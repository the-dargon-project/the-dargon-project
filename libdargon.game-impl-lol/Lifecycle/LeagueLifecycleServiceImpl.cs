using System;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.Trinkets.Commands;
using Dargon.Trinkets.Spawner;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Processes;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dargon.LeagueOfLegends.Utilities;
using Dargon.Modifications;
using PhaseChange = System.Tuple<Dargon.LeagueOfLegends.Session.LeagueSessionPhase, Dargon.LeagueOfLegends.Session.LeagueSessionPhase>;
using PhaseChangeHandler = System.Action<Dargon.LeagueOfLegends.Session.ILeagueSession, Dargon.LeagueOfLegends.Session.LeagueSessionPhaseChangedArgs>;

namespace Dargon.LeagueOfLegends.Lifecycle {
   public class LeagueLifecycleServiceImpl : LeagueLifecycleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ConcurrentDictionary<string, CompletionChain> resolutionChainsByModificationName = new ConcurrentDictionary<string, CompletionChain>();
      private readonly ConcurrentDictionary<string, CompletionChain> compilationChainsByModificationName = new ConcurrentDictionary<string, CompletionChain>();
      private readonly TrinketSpawner trinketSpawner;
      private readonly LeagueBuildUtilities leagueBuildUtilities;
      private readonly LeagueSessionService leagueSessionService;
      private readonly RadsService radsService;
      private readonly LeagueTrinketSpawnConfigurationFactory leagueTrinketSpawnConfigurationFactory;
      private readonly ModificationLoader modificationLoader;
      private readonly IReadOnlyDictionary<PhaseChange, PhaseChangeHandler> phaseChangeHandlers;
      private readonly IReadOnlyDictionary<LeagueProcessType, LeagueSessionProcessLaunchedHandler> processLaunchedHandlers;

      public LeagueLifecycleServiceImpl(TrinketSpawner trinketSpawner, LeagueBuildUtilities leagueBuildUtilities, LeagueSessionService leagueSessionService, RadsService radsService, LeagueTrinketSpawnConfigurationFactory leagueTrinketSpawnConfigurationFactory, ModificationLoader modificationLoader)
      {
         this.trinketSpawner = trinketSpawner;
         this.leagueBuildUtilities = leagueBuildUtilities;
         this.leagueSessionService = leagueSessionService;
         this.radsService = radsService;
         this.leagueTrinketSpawnConfigurationFactory = leagueTrinketSpawnConfigurationFactory;
         this.modificationLoader = modificationLoader;

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
         var modifications = modificationLoader.EnumerateModifications();
         foreach (var modification in modifications) {
            var resolutionChain = resolutionChainsByModificationName.GetOrAdd(
               modification.RepositoryName, 
               add => new CompletionChain(cancellationToken => leagueBuildUtilities.ResolveModification(modification, cancellationToken))
            );
            var compilationChain = compilationChainsByModificationName.GetOrAdd(
               modification.RepositoryName,
               add => new CompletionChain(cancellationToken => leagueBuildUtilities.CompileModification(modification, cancellationToken))
            );
            var resolutionLink = resolutionChain.CreateLink("resolution_" + DateTime.Now.ToFileTimeUtc());
            var compilationLink = compilationChain.CreateLink("compilation_" + DateTime.Now.ToFileTimeUtc());
            resolutionLink.Tail(compilationLink.StartAndWaitForChain);
            resolutionChain.StartNext(resolutionLink);
         }
         radsService.Suspend();
      }

      internal void HandlePreclientToClientPhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e) {
         logger.Info("Handling Preclient to Client Phase Transition!");
         radsService.Resume();

         var modifications = modificationLoader.EnumerateModifications();
         int compilationsRemaining = modifications.Count;
         foreach (var modification in modifications) {
            var resolutionChain = resolutionChainsByModificationName.GetOrAdd(
               modification.RepositoryName,
               add => new CompletionChain(cancellationToken => leagueBuildUtilities.ResolveModification(modification, cancellationToken))
            );
            var compilationChain = compilationChainsByModificationName.GetOrAdd(
               modification.RepositoryName,
               add => new CompletionChain(cancellationToken => leagueBuildUtilities.CompileModification(modification, cancellationToken))
            );
            var resolutionLink = resolutionChain.CreateLink("resolution_" + DateTime.Now.ToFileTimeUtc());
            var compilationLink = compilationChain.CreateLink("compilation_" + DateTime.Now.ToFileTimeUtc());
            resolutionLink.Tail(compilationLink.StartAndWaitForChain);
            compilationLink.Tail(() => {
               logger.Info("Compilation counter at " + compilationsRemaining);
               if (Interlocked.Decrement(ref compilationsRemaining) == 0) {
                  var airClientCommands = leagueBuildUtilities.LinkAirModifications(modifications);
                  trinketSpawner.SpawnTrinket(
                     session.GetProcessOrNull(LeagueProcessType.PvpNetClient),
                     leagueTrinketSpawnConfigurationFactory.GetClientConfiguration(airClientCommands)
                  );
               }
            });
            resolutionChain.StartNext(resolutionLink);
         }
      }

      internal void HandleClientToGamePhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Client to Game Phase Transition!");

         var modifications = modificationLoader.EnumerateModifications();
         int compilationsRemaining = modifications.Count;
         foreach (var modification in modifications) {
            var resolutionChain = resolutionChainsByModificationName.GetOrAdd(
               modification.RepositoryName,
               add => new CompletionChain(cancellationToken => leagueBuildUtilities.ResolveModification(modification, cancellationToken))
            );
            var compilationChain = compilationChainsByModificationName.GetOrAdd(
               modification.RepositoryName,
               add => new CompletionChain(cancellationToken => leagueBuildUtilities.CompileModification(modification, cancellationToken))
            );
            var resolutionLink = resolutionChain.CreateLink("resolution_" + DateTime.Now.ToFileTimeUtc());
            var compilationLink = compilationChain.CreateLink("compilation_" + DateTime.Now.ToFileTimeUtc());
            resolutionLink.Tail(compilationLink.StartAndWaitForChain);
            compilationLink.Tail(() => {
               logger.Info("Compilation counter at " + compilationsRemaining);
               if (Interlocked.Decrement(ref compilationsRemaining) == 0) {
                  var gameModifications = leagueBuildUtilities.LinkGameModifications(modifications);
                  trinketSpawner.SpawnTrinket(
                     session.GetProcessOrNull(LeagueProcessType.GameClient),
                     leagueTrinketSpawnConfigurationFactory.GetGameConfiguration(gameModifications)
                  );
               }
            });
            resolutionChain.StartNext(resolutionLink);
         }
      }
   }
}
