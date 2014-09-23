using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.LeagueOfLegends.Session;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class DerpyLifecycleServiceImpl
   {
      private readonly LeagueSessionWatcherService leagueSessionWatcherService;

      public DerpyLifecycleServiceImpl(LeagueSessionWatcherService leagueSessionWatcherService) {
         this.leagueSessionWatcherService = leagueSessionWatcherService;
      }
   }
}
