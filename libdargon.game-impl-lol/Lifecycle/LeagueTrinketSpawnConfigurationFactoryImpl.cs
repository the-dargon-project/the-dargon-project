using Dargon.Trinkets.Commands;
using Dargon.Trinkets.Components;
using Dargon.Trinkets.Spawner;
using ItzWarty.Collections;
using SCG = System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Lifecycle {
   public class LeagueTrinketSpawnConfigurationFactoryImpl : LeagueTrinketSpawnConfigurationFactory
   {
      public TrinketSpawnConfiguration GetPreclientConfiguration()
      {
         var suspendedProcessNames = new HashSet<string> { "LolClient.exe", "League of Legends.exe" };
         return new TrinketSpawnConfigurationImpl {
            Name = "LAUNCHER",
            IsDebugEnabled = true,
            IsProcessSuspensionEnabled = true,
            SuspendedProcessNames = suspendedProcessNames,
            IsLoggingEnabled = true
         };
      }

      public TrinketSpawnConfiguration GetClientConfiguration(CommandList commandList) {
         return new TrinketSpawnConfigurationImpl {
            Name = "CLIENT",
            IsDebugEnabled = true,
            IsCommandingEnabled = true,
            CommandList = commandList,
            IsFileSystemHookingEnabled = true,
            IsFileSystemOverridingEnabled = true,
            IsLoggingEnabled = true
         };
      }

      public TrinketSpawnConfiguration GetGameConfiguration(CommandList commandList) {
         return new TrinketSpawnConfigurationImpl {
            Name = "GAME",
            IsDebugEnabled = true,
            IsCommandingEnabled = true,
            CommandList = commandList,
            IsFileSystemHookingEnabled = true,
            IsFileSystemOverridingEnabled = true,
            IsLoggingEnabled = true
         };
      }
   }
}