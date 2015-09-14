using Dargon.LeagueOfLegends.FileSystem;
using Dargon.LeagueOfLegends.Lifecycle;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.RADS;
using Dargon.Ryu;
using Dargon.Trinkets.Commands;

namespace Dargon.LeagueOfLegends {
   public class LeagueImplRyuPackage : RyuPackageV1 {
      public LeagueImplRyuPackage() {
         Singleton<LeagueConfiguration>();
         Singleton<RadsServiceImpl>(ryu => new RadsServiceImpl(ryu.Get<LeagueConfiguration>().RadsPath));
         Singleton<RadsService, RadsServiceImpl>();
         Singleton<LeagueProcessWatcherService, LeagueProcessWatcherServiceImpl>(RyuTypeFlags.Required);
         Singleton<LeagueSessionService, LeagueSessionServiceImpl>(RyuTypeFlags.Required);
         Singleton<RiotFileSystem>();
         Singleton<LeagueTrinketSpawnConfigurationFactory, LeagueTrinketSpawnConfigurationFactoryImpl>();
         Singleton<LeagueBuildUtilitiesConfiguration>();
         Singleton<LeagueBuildUtilities>();
         Singleton<LeagueLifecycleService, LeagueLifecycleServiceImpl>(RyuTypeFlags.Required);
         Mob<LeagueModificationsMob>();
      }
   }
}
