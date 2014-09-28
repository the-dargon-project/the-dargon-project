using System.Linq;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Modifications;
using ItzWarty.Collections;
using NLog;
using System.Collections.Generic;
using PhaseChange = System.Tuple<Dargon.LeagueOfLegends.Session.LeagueSessionPhase, Dargon.LeagueOfLegends.Session.LeagueSessionPhase>;
using PhaseChangeHandler = System.Action<Dargon.LeagueOfLegends.Session.ILeagueSession, Dargon.LeagueOfLegends.Session.LeagueSessionPhaseChangedArgs>;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class LeagueLifecycleServiceImpl : LeagueLifecycleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      private readonly LeagueModificationCompilationService leagueModificationCompilationService;
      private readonly LeagueSessionWatcherService leagueSessionWatcherService;
      private readonly RadsService radsService;
      private readonly IReadOnlyDictionary<PhaseChange, PhaseChangeHandler> phaseChangeHandlers;

      public LeagueLifecycleServiceImpl(LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueModificationResolutionService leagueModificationResolutionService, LeagueModificationCompilationService leagueModificationCompilationService, LeagueSessionWatcherService leagueSessionWatcherService, RadsServiceImpl radsService)
      {
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
      }

      private void HandleLeagueSessionCreated(LeagueSessionWatcherService service, LeagueSessionCreatedArgs e)
      {
         var session = e.Session;
         session.PhaseChanged += HandleSessionPhaseChanged;
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

         // TODO: Inject
      }

      private void HandlePreclientToClientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Preclient to Client Phase Transition!");
         radsService.Resume();

         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
         var resolutionTasks = ResolveAllModifications(mods, ModificationTargetType.Client | ModificationTargetType.Game);
         WaitForCancellableTaskCompletion(resolutionTasks);
         var clientCompilationTasks = CompileAllModifications(mods, ModificationTargetType.Client);
         WaitForCancellableTaskCompletion(clientCompilationTasks);
         BuildLeagueIndexFiles();
         // TODO: Inject

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
