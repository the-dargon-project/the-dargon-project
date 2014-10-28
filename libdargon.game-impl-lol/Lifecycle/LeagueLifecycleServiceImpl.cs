using System;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Tasks;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Modifications;
using ItzWarty;
using ItzWarty.Collections;
using NLog;
using System.Collections.Generic;
using System.Linq;
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
      private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService;
      private readonly LeagueModificationTasklistCompilerService leagueModificationTasklistCompilerService;
      private readonly LeagueGameModificationLinkerService leagueGameModificationLinkerService;
      private readonly LeagueSessionService leagueSessionService;
      private readonly RadsService radsService;
      private readonly ILeagueInjectedModuleConfigurationFactory leagueInjectedModuleConfigurationFactory;
      private readonly IReadOnlyDictionary<PhaseChange, PhaseChangeHandler> phaseChangeHandlers;
      private readonly IReadOnlyDictionary<LeagueProcessType, LeagueSessionProcessLaunchedHandler> processLaunchedHandlers;

      public LeagueLifecycleServiceImpl(InjectedModuleService injectedModuleService, LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueModificationResolutionService leagueModificationResolutionService, LeagueModificationObjectCompilerService leagueModificationObjectCompilerService, LeagueModificationTasklistCompilerService leagueModificationTasklistCompilerService, LeagueGameModificationLinkerService leagueGameModificationLinkerService, LeagueSessionService leagueSessionService, RadsService radsService, ILeagueInjectedModuleConfigurationFactory leagueInjectedModuleConfigurationFactory)
      {
         this.injectedModuleService = injectedModuleService;
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
         this.leagueModificationResolutionService = leagueModificationResolutionService;
         this.leagueModificationObjectCompilerService = leagueModificationObjectCompilerService;
         this.leagueModificationTasklistCompilerService = leagueModificationTasklistCompilerService;
         this.leagueGameModificationLinkerService = leagueGameModificationLinkerService;
         this.leagueSessionService = leagueSessionService;
         this.radsService = radsService;
         this.leagueInjectedModuleConfigurationFactory = leagueInjectedModuleConfigurationFactory;

         phaseChangeHandlers = ImmutableDictionary.Of<PhaseChange, PhaseChangeHandler>(
            new PhaseChange(LeagueSessionPhase.Uninitialized, LeagueSessionPhase.Preclient), HandleUninitializedToPreclientPhaseTransition,
            new PhaseChange(LeagueSessionPhase.Preclient, LeagueSessionPhase.Client), HandlePreclientToClientPhaseTransition
         );
         processLaunchedHandlers = ImmutableDictionary.Of<LeagueProcessType, LeagueSessionProcessLaunchedHandler>(
            LeagueProcessType.RadsUserKernel, (s, e) => HandlePreclientProcessLaunched(e.Process.Id),
            LeagueProcessType.Launcher, (s, e) => HandlePreclientProcessLaunched(e.Process.Id),
            LeagueProcessType.Patcher, (s, e) => HandlePreclientProcessLaunched(e.Process.Id)
         );
      }

      public void Initialize()
      {
         leagueSessionService.SessionCreated += HandleLeagueSessionCreated;
      }

      private void HandleLeagueSessionCreated(LeagueSessionService service, LeagueSessionCreatedArgs e)
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

      internal void HandlePreclientProcessLaunched(int processId)
      {
         injectedModuleService.InjectToProcess(processId, leagueInjectedModuleConfigurationFactory.GetPreclientConfiguration());
      }

      internal void HandleSessionPhaseChanged(ILeagueSession session, LeagueSessionPhaseChangedArgs e) 
      { 
         logger.Info("Phase Change from " + e.Previous + " to " + e.Current);
         PhaseChangeHandler handler;
         if (phaseChangeHandlers.TryGetValue(new PhaseChange(e.Previous, e.Current), out handler)) {
            handler(session, e);
         }
      }

      internal void HandleUninitializedToPreclientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Uninitialized to Preclient Phase Transition!");
         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();

         var resolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Client)).ToList();
         radsService.Suspend();
         resolutionTasks.ForEach(task => task.WaitForChainCompletion());
         
         var compilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client)).ToList();
         compilationTasks.ForEach(task => task.WaitForChainCompletion());
      }

      internal void HandlePreclientToClientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Preclient to Client Phase Transition!");
         radsService.Resume();

         var tasklist = new LateInitializationTasklistProxy();
         injectedModuleService.InjectToProcess(session.GetProcessOrNull(LeagueProcessType.PvpNetClient).Id, leagueInjectedModuleConfigurationFactory.GetClientConfiguration(tasklist));

         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
         var clientResolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Client)).ToList();
         clientResolutionTasks.ForEach(task => task.WaitForChainCompletion());

         var clientCompilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client)).ToList();
         clientCompilationTasks.ForEach(task => task.WaitForChainCompletion());

         tasklist.SetTasklist(mods.Aggregate(new Tasklist(), (tl, mod) => tl.AddRange(leagueModificationTasklistCompilerService.BuildTasklist(mod, ModificationTargetType.Client))));

         // optimization: compile game data here, so that we don't have to compile when game starts
         var gameResolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Game)).ToList();
         gameResolutionTasks.ForEach(task => task.WaitForChainCompletion());

         var gameCompilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Game)).ToList();
         gameCompilationTasks.ForEach(task => task.WaitForChainCompletion());

         leagueGameModificationLinkerService.LinkModificationObjects();
      }

      internal void HandleClientToGamePhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Client to Game Phase Transition!");

         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();

         var gameResolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Game)).ToList();
         gameResolutionTasks.ForEach(task => task.WaitForChainCompletion());

         var gameCompilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Game)).ToList();
         gameCompilationTasks.ForEach(task => task.WaitForChainCompletion());

         leagueGameModificationLinkerService.LinkModificationObjects();
      }
   }
}
