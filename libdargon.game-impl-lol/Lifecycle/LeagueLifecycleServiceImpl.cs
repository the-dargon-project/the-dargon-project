using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Trinkets.Commands;
using ItzWarty.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Dargon.Trinkets.Spawner;
using ItzWarty;
using ItzWarty.Processes;
using PhaseChange = System.Tuple<Dargon.LeagueOfLegends.Session.LeagueSessionPhase, Dargon.LeagueOfLegends.Session.LeagueSessionPhase>;
using PhaseChangeHandler = System.Action<Dargon.LeagueOfLegends.Session.ILeagueSession, Dargon.LeagueOfLegends.Session.LeagueSessionPhaseChangedArgs>;

namespace Dargon.LeagueOfLegends.Lifecycle {
   public class LeagueLifecycleServiceImpl : LeagueLifecycleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly TrinketSpawner trinketSpawner;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService;
      private readonly LeagueModificationCommandListCompilerService leagueModificationCommandListCompilerService;
      private readonly LeagueGameModificationLinkerService leagueGameModificationLinkerService;
      private readonly LeagueSessionService leagueSessionService;
      private readonly RadsService radsService;
      private readonly LeagueTrinketSpawnConfigurationFactory leagueTrinketSpawnConfigurationFactory;
      private readonly IReadOnlyDictionary<PhaseChange, PhaseChangeHandler> phaseChangeHandlers;
      private readonly IReadOnlyDictionary<LeagueProcessType, LeagueSessionProcessLaunchedHandler> processLaunchedHandlers;

      public LeagueLifecycleServiceImpl(TrinketSpawner trinketSpawner, LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueModificationResolutionService leagueModificationResolutionService, LeagueModificationObjectCompilerService leagueModificationObjectCompilerService, LeagueModificationCommandListCompilerService leagueModificationCommandListCompilerService, LeagueGameModificationLinkerService leagueGameModificationLinkerService, LeagueSessionService leagueSessionService, RadsService radsService, LeagueTrinketSpawnConfigurationFactory leagueTrinketSpawnConfigurationFactory)
      {
         this.trinketSpawner = trinketSpawner;
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
         this.leagueModificationResolutionService = leagueModificationResolutionService;
         this.leagueModificationObjectCompilerService = leagueModificationObjectCompilerService;
         this.leagueModificationCommandListCompilerService = leagueModificationCommandListCompilerService;
         this.leagueGameModificationLinkerService = leagueGameModificationLinkerService;
         this.leagueSessionService = leagueSessionService;
         this.radsService = radsService;
         this.leagueTrinketSpawnConfigurationFactory = leagueTrinketSpawnConfigurationFactory;

         phaseChangeHandlers = ImmutableDictionary.Of<PhaseChange, PhaseChangeHandler>(
            new PhaseChange(LeagueSessionPhase.Uninitialized, LeagueSessionPhase.Preclient), HandleUninitializedToPreclientPhaseTransition,
            new PhaseChange(LeagueSessionPhase.Preclient, LeagueSessionPhase.Client), HandlePreclientToClientPhaseTransition,
            new PhaseChange(LeagueSessionPhase.Client, LeagueSessionPhase.Game), HandleClientToGamePhaseTransition
         );
         processLaunchedHandlers = ImmutableDictionary.Of<LeagueProcessType, LeagueSessionProcessLaunchedHandler>(
            LeagueProcessType.RadsUserKernel, (s, e) => HandlePreclientProcessLaunched(e.Process),
            LeagueProcessType.Launcher, (s, e) => HandlePreclientProcessLaunched(e.Process),
            LeagueProcessType.Patcher, (s, e) => HandlePreclientProcessLaunched(e.Process)
         );
      }

      public void Initialize()
      {
         leagueSessionService.SessionCreated += HandleLeagueSessionCreated;
      }

      internal void HandleLeagueSessionCreated(LeagueSessionService service, LeagueSessionCreatedArgs e)
      {
         var session = e.Session;
         session.PhaseChanged += HandleSessionPhaseChanged;
         session.ProcessLaunched += HandleSessionProcessLaunched;
      }

      internal void HandleSessionProcessLaunched(ILeagueSession session, LeagueSessionProcessLaunchedArgs e) 
      { 
         logger.Info("Process Launched " + e.Type + ": " + e.Process.Id);
         LeagueSessionProcessLaunchedHandler handler;
         if (processLaunchedHandlers.TryGetValue(e.Type, out handler)) {
            handler(session, e);
         }
      }

      internal void HandlePreclientProcessLaunched(IProcess process)
      {
         trinketSpawner.SpawnTrinket(
            process,
            leagueTrinketSpawnConfigurationFactory.GetPreclientConfiguration()
         );
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

      internal void HandlePreclientToClientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e) {
         logger.Info("Handling Preclient to Client Phase Transition!");
         radsService.Resume();

         
         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
         var clientResolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Client)).ToList();
         clientResolutionTasks.ForEach(task => task.WaitForChainCompletion());

         var clientCompilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client)).ToList();
         clientCompilationTasks.ForEach(task => task.WaitForChainCompletion());

         var commandList = mods.Aggregate(new DefaultCommandList(), (l, mod) => l.With(x => x.AddRange(leagueModificationCommandListCompilerService.BuildCommandList(mod, ModificationTargetType.Client))));

         trinketSpawner.SpawnTrinket(
            session.GetProcessOrNull(LeagueProcessType.PvpNetClient),
            leagueTrinketSpawnConfigurationFactory.GetClientConfiguration(commandList)
         );

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

         //         injectedModuleService.InjectToProcess(session.GetProcessOrNull(LeagueProcessType.GameClient).Id, leagueInjectedModuleConfigurationFactory.GetGameConfiguration(commandList));

         var mods = leagueModificationRepositoryService.EnumerateModifications().ToList();
         var gameResolutionTasks = mods.Select(mod => leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Game)).ToList();
         gameResolutionTasks.ForEach(task => task.WaitForChainCompletion());

         var gameCompilationTasks = mods.Select(mod => leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Game)).ToList();
         gameCompilationTasks.ForEach(task => task.WaitForChainCompletion());

         var commands = leagueGameModificationLinkerService.LinkModificationObjects();

         trinketSpawner.SpawnTrinket(
            session.GetProcessOrNull(LeagueProcessType.GameClient),
            leagueTrinketSpawnConfigurationFactory.GetGameConfiguration(commands)
         );
      }
   }
}
