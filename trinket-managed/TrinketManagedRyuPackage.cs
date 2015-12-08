using System.Diagnostics;
using Dargon.RADS;
using Dargon.Ryu;
using Dargon.Trinkets.Hosted.Hooks;
using Dargon.Trinkets.Hosted.Management;
using Dargon.Trinkets.Hosted.Management.Air;
using Dargon.Trinkets.Hosted.Management.Game;
using ItzWarty.IO;

namespace Dargon.Trinkets.Hosted {
   public class TrinketManagedRyuPackage : RyuPackageV1 {
      public TrinketManagedRyuPackage() {
         Singleton<FileSystemHookEventBusImpl>();
         Singleton<FileSystemHookEventBus, FileSystemHookEventBusImpl>();

         Singleton<Direct3D9HookEventBusImpl>();
         Singleton<Direct3D9HookEventBus, Direct3D9HookEventBusImpl>();

         Singleton<TrinketIoUtilities>();
         Singleton<LeagueRadsWatcher>(ConstructLeagueRadsWatcher);

         Singleton<LeagueTextureWatcher>();

         Singleton<LeagueTextureSwapper>();
         GameMob<LeagueTextureSwapperMob>();

         AirMob<AirDebugMob>();
      }

      private void AirMob<TMob>() {
         if (LeagueTrinketRoleUtilities.GetRole() == LeagueTrinketRole.Air) {
            Mob<TMob>();
         }
      }

      private void GameMob<TMob>() {
         if (LeagueTrinketRoleUtilities.GetRole() == LeagueTrinketRole.Game) {
            Mob<TMob>();
         }
      }

      private LeagueRadsWatcher ConstructLeagueRadsWatcher(RyuContainer ryu) {
         var leagueTrinketConfiguration = ryu.Get<LeagueTrinketConfiguration>();
         var riotProjectLoader = new RiotProjectLoader(ryu.Get<StreamFactory>());
         var riotSolutionLoader = new RiotSolutionLoader(riotProjectLoader);
         var riotSolution = riotSolutionLoader.Load(leagueTrinketConfiguration.RadsPath);
         var gameProject = riotSolution.ProjectsByType[RiotProjectType.GameClient];
         var watcher = new LeagueRadsWatcher(
            ryu.Get<TrinketIoUtilities>(),
            ryu.Get<FileSystemHookEventBus>(),
            leagueTrinketConfiguration,
            gameProject);
         watcher.Initialize();
         return watcher;
      }
   }
}
