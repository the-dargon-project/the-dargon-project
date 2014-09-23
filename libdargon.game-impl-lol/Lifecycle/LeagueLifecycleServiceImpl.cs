using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Session;
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
      private readonly LeagueSessionWatcherService leagueSessionWatcherService;
      private readonly IReadOnlyDictionary<PhaseChange, PhaseChangeHandler> phaseChangeHandlers;

      public LeagueLifecycleServiceImpl(LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueSessionWatcherService leagueSessionWatcherService)
      {
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
         this.leagueSessionWatcherService = leagueSessionWatcherService;
         leagueSessionWatcherService.SessionCreated += HandleLeagueSessionCreated;
         phaseChangeHandlers = ImmutableDictionary.Of<PhaseChange, PhaseChangeHandler>(
            new PhaseChange(LeagueSessionPhase.Uninitialized, LeagueSessionPhase.Preclient), HandleUninitializedToPreclientPhaseTransition
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
         var mods = leagueModificationRepositoryService.EnumerateModifications();
         foreach (var mod in mods) {
         }
      }
   }
}
