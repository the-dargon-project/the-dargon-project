using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.LeagueOfLegends.Session;
using NLog;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class LeagueLifecycleServiceImpl : LeagueLifecycleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueSessionWatcherService leagueSessionWatcherService;

      public LeagueLifecycleServiceImpl(LeagueSessionWatcherService leagueSessionWatcherService) {
         this.leagueSessionWatcherService = leagueSessionWatcherService;
         leagueSessionWatcherService.SessionCreated += HandleLeagueSessionCreated;
      }

      private void HandleLeagueSessionCreated(LeagueSessionWatcherService service, LeagueSessionCreatedArgs e)
      {
         var session = e.Session;
         session.PhaseChanged += HandleSessionPhaseChanged;
      }

      private void HandleSessionPhaseChanged(ILeagueSession session, LeagueSessionPhaseChangedArgs e) 
      { 
         logger.Info("Phase Change from " + e.Previous + " to " + e.Current);
      }
   }
}
