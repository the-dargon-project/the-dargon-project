using System.Linq;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Components;
using Dargon.InjectedModule.Tasklist;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Modifications;
using ItzWarty.Collections;
using NLog;
using System.Collections.Generic;
using ITask = Dargon.LeagueOfLegends.Modifications.ITask;
using PhaseChange = System.Tuple<Dargon.LeagueOfLegends.Session.LeagueSessionPhase, Dargon.LeagueOfLegends.Session.LeagueSessionPhase>;
using PhaseChangeHandler = System.Action<Dargon.LeagueOfLegends.Session.ILeagueSession, Dargon.LeagueOfLegends.Session.LeagueSessionPhaseChangedArgs>;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class LeagueLifecycleServiceImpl : LeagueLifecycleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly InjectedModuleService injectedModuleService;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      private readonly LeagueModificationCompilationService leagueModificationCompilationService;
      private readonly LeagueSessionWatcherService leagueSessionWatcherService;
      private readonly RadsService radsService;
      private readonly IReadOnlyDictionary<PhaseChange, PhaseChangeHandler> phaseChangeHandlers;
      private readonly IReadOnlyDictionary<LeagueProcessType, LeagueSessionProcessLaunchedHandler> processLaunchedHandlers;

      public LeagueLifecycleServiceImpl(InjectedModuleService injectedModuleService, LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueModificationResolutionService leagueModificationResolutionService, LeagueModificationCompilationService leagueModificationCompilationService, LeagueSessionWatcherService leagueSessionWatcherService, RadsServiceImpl radsService)
      {
         this.injectedModuleService = injectedModuleService;
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
         this.leagueModificationResolutionService = leagueModificationResolutionService;
         this.leagueModificationCompilationService = leagueModificationCompilationService;
         this.leagueSessionWatcherService = leagueSessionWatcherService;
         this.radsService = radsService;

         leagueSessionWatcherService.SessionCreated += HandleLeagueSessionCreated;

         phaseChangeHandlers = ImmutableDictionary.Of<PhaseChange, PhaseChangeHandler>(
            new PhaseChange(LeagueSessionPhase.Uninitialized, LeagueSessionPhase.Preclient), HandleUninitializedToPreclientPhaseTransition,
            new PhaseChange(LeagueSessionPhase.Preclient, LeagueSessionPhase.Client), HandlePreclientToClientPhaseTransition
         );
         processLaunchedHandlers = ImmutableDictionary.Of<LeagueProcessType, LeagueSessionProcessLaunchedHandler>(
            LeagueProcessType.RadsUserKernel, HandlePreclientProcessLaunched,
            LeagueProcessType.Launcher, HandlePreclientProcessLaunched,
            LeagueProcessType.Patcher, HandlePreclientProcessLaunched
         );
      }

      private void HandleLeagueSessionCreated(LeagueSessionWatcherService service, LeagueSessionCreatedArgs e)
      {
         var session = e.Session;
         session.PhaseChanged += HandleSessionPhaseChanged;
         session.ProcessLaunched += HandleSessionProcessLaunched;
      }

      private void HandleSessionProcessLaunched(ILeagueSession session, LeagueSessionProcessLaunchedArgs e) 
      { 
         logger.Info("Process Launched " + e.Type + ": " + e.Process.Id);
         LeagueSessionProcessLaunchedHandler handler;
         if (processLaunchedHandlers.TryGetValue(e.Type, out handler)) {
            handler(session, e);
         }
      }

      private void HandlePreclientProcessLaunched(ILeagueSession session, LeagueSessionProcessLaunchedArgs e)
      {
         var suspendedProcessNames = new HashSet<string> { "LolClient.exe", "League of Legends.exe" };
         var configurationBuilder = new InjectedModuleConfigurationBuilder();
         configurationBuilder.AddComponent(new DebugConfigurationComponent());
         configurationBuilder.AddComponent(new RoleConfigurationComponent(DimRole.Launcher));
         configurationBuilder.AddComponent(new ProcessSuspensionConfigurationComponent(suspendedProcessNames));
         configurationBuilder.AddComponent(new VerboseLoggerConfigurationComponent());
         
         injectedModuleService.InjectToProcess(e.Process.Id, configurationBuilder.Build());
      }

      private void HandleSessionPhaseChanged(ILeagueSession session, LeagueSessionPhaseChangedArgs e) 
      { 
         logger.Info("Phase Change from " + e.Previous + " to " + e.Current);
         PhaseChangeHandler handler;
         if (phaseChangeHandlers.TryGetValue(new PhaseChange(e.Previous, e.Current), out handler)) {
            handler(session, e);
         }
      }

      private void HandleUninitializedToPreclientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Uninitialized to Preclient Phase Transition!");
         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
         var resolutionTasks = ResolveAllModifications(mods, ModificationTargetType.Client);

         radsService.Suspend();

         WaitForCancellableTaskCompletion(resolutionTasks);
         var clientCompilationTasks = CompileAllModifications(mods, ModificationTargetType.Client);
         WaitForCancellableTaskCompletion(clientCompilationTasks);
      }

      private void HandlePreclientToClientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Preclient to Client Phase Transition!");
         radsService.Resume();

         var lateInitializedTasklist = new LateInitializationTasklist();
         var configurationBuilder = new InjectedModuleConfigurationBuilder();
         configurationBuilder.AddComponent(new DebugConfigurationComponent());
         configurationBuilder.AddComponent(new RoleConfigurationComponent(DimRole.Client));
         configurationBuilder.AddComponent(new TasklistConfigurationComponent(lateInitializedTasklist));
         configurationBuilder.AddComponent(new FilesystemConfigurationComponent(true));
         configurationBuilder.AddComponent(new VerboseLoggerConfigurationComponent());
         
         injectedModuleService.InjectToProcess(session.GetProcessOrNull(LeagueProcessType.PvpNetClient).Id, configurationBuilder.Build());

         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
         var resolutionTasks = ResolveAllModifications(mods, ModificationTargetType.Client | ModificationTargetType.Game);
         WaitForCancellableTaskCompletion(resolutionTasks);
         var clientCompilationTasks = CompileAllModifications(mods, ModificationTargetType.Client);
         WaitForCancellableTaskCompletion(clientCompilationTasks);

         // optimization: compile game data here, so that we don't have to compile when game starts
         var gameCompilationTasks = CompileAllModifications(mods, ModificationTargetType.Game);
         WaitForCancellableTaskCompletion(gameCompilationTasks);
         BuildLeagueIndexFiles();
      }

      private List<IResolutionTask> ResolveAllModifications(List<IModification> mods, ModificationTargetType targetType)
      {
         var resolutionTasks = new List<IResolutionTask>(mods.Count);
         foreach (var mod in mods) {
            resolutionTasks.Add(leagueModificationResolutionService.ResolveModification(mod, targetType));
         }
         return resolutionTasks;
      }

      private List<ICompilationTask> CompileAllModifications(List<IModification> mods, ModificationTargetType target)
      {
         var compilationTasks = new List<ICompilationTask>(mods.Count);
         foreach (var mod in mods)
         {
            compilationTasks.Add(leagueModificationCompilationService.CompileModification(mod, target));
         }
         return compilationTasks;
      }

      private void BuildLeagueIndexFiles() { }

      private static void WaitForCancellableTaskCompletion(IEnumerable<ITask> resolutionTasks)
      {
         foreach (var task in resolutionTasks) {
            var currentTask = task;
            bool done = false;
            while (!done) {
               currentTask.WaitForTermination();
               if (currentTask.Status == Status.Cancelled) {
                  currentTask = currentTask.NextTask;
                  if (currentTask == null) {
                     logger.Warn("Warning: resolution task " + currentTask + " cancelled and has no next resolution task");
                     done = true;
                  }
               } else if (currentTask.Status == Status.Completed) {
                  done = true;
               }
            }
         }
      }
   }
}
