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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

      [DllImport("kernel32.dll", SetLastError = true)]
      static extern int SuspendThread(IntPtr hThread);

      [DllImport("kernel32.dll")]
      static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

      [DllImport("kernel32.dll")]
      static extern uint ResumeThread(IntPtr hThread);

      [Flags]
      public enum ThreadAccess : int {
         TERMINATE = (0x0001),
         SUSPEND_RESUME = (0x0002),
         GET_CONTEXT = (0x0008),
         SET_CONTEXT = (0x0010),
         SET_INFORMATION = (0x0020),
         QUERY_INFORMATION = (0x0040),
         SET_THREAD_TOKEN = (0x0080),
         IMPERSONATE = (0x0100),
         DIRECT_IMPERSONATION = (0x0200)
      }

      internal void HandleSessionProcessLaunched(ILeagueSession session, LeagueSessionProcessLaunchedArgs e) 
      { 
         logger.Info("Process Launched " + e.Type + ": " + e.Process.Id);
         LeagueSessionProcessLaunchedHandler handler;
         if (processLaunchedHandlers.TryGetValue(e.Type, out handler)) {
            handler(session, e);
         }
      }

      internal void HandlePreclientProcessLaunched(IProcess processInput) {
         var process = Process.GetProcessById(processInput.Id);
         var threads = process.Threads.Cast<ProcessThread>().ToArray();
         var suspendedThreadsHandles = threads.Select(thread => OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id)).ToArray();

         foreach (var suspendedThreadHandle in suspendedThreadsHandles) {
            SuspendThread(suspendedThreadHandle);
         }

         trinketSpawner.SpawnTrinket(
            processInput,
            leagueTrinketSpawnConfigurationFactory.GetPreclientConfiguration()
         );

         foreach (var suspendedThreadHandle in suspendedThreadsHandles) {
            ResumeThread(suspendedThreadHandle);
         }
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
         var spawnTrinketAction = new Action(() => {
            var airClientCommands = leagueBuildUtilities.LinkAirModifications(modifications);
            trinketSpawner.SpawnTrinket(
               session.GetProcessOrNull(LeagueProcessType.PvpNetClient),
               leagueTrinketSpawnConfigurationFactory.GetClientConfiguration(airClientCommands)
            );
         });
         if (modifications.None()) {
            spawnTrinketAction();
         } else {
            int compilationsRemaining = modifications.Count;
            foreach (var modification in modifications) {
               var resolutionChain = resolutionChainsByModificationName.GetOrAdd(
                  modification.RepositoryName,
                  add => new CompletionChain(cancellationToken => leagueBuildUtilities.ResolveModification(modification, cancellationToken)));
               var compilationChain = compilationChainsByModificationName.GetOrAdd(
                  modification.RepositoryName,
                  add => new CompletionChain(cancellationToken => leagueBuildUtilities.CompileModification(modification, cancellationToken)));
               var resolutionLink = resolutionChain.CreateLink("resolution_" + DateTime.Now.ToFileTimeUtc());
               var compilationLink = compilationChain.CreateLink("compilation_" + DateTime.Now.ToFileTimeUtc());
               resolutionLink.Tail(compilationLink.StartAndWaitForChain);
               compilationLink.Tail(() => {
                  logger.Info("Compilation counter at " + compilationsRemaining);
                  if (Interlocked.Decrement(ref compilationsRemaining) == 0) {
                     spawnTrinketAction();
                  }
               });
               resolutionChain.StartNext(resolutionLink);
            }
         }
      }

      internal void HandleClientToGamePhaseTransition(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         logger.Info("Handling Client to Game Phase Transition!");

         var modifications = modificationLoader.EnumerateModifications();
         var spawnTrinketAction = new Action(() => {
            var gameModifications = leagueBuildUtilities.LinkGameModifications(modifications);
            trinketSpawner.SpawnTrinket(
               session.GetProcessOrNull(LeagueProcessType.GameClient),
               leagueTrinketSpawnConfigurationFactory.GetGameConfiguration(gameModifications));
         });
         if (modifications.None()) {
            spawnTrinketAction();
         } else {
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
                  if (Interlocked.Decrement(ref compilationsRemaining) == 0) {}
               });
               resolutionChain.StartNext(resolutionLink);
            }
         }
      }
   }
}
