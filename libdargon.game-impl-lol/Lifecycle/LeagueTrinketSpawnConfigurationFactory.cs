using Dargon.Trinkets.Commands;
using Dargon.Trinkets.Components;
using Dargon.Trinkets.Spawner;
using SCG = System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Lifecycle {
   public interface LeagueTrinketSpawnConfigurationFactory
   {
      TrinketSpawnConfiguration GetPreclientConfiguration();
      TrinketSpawnConfiguration GetClientConfiguration(CommandList commandList);
      TrinketSpawnConfiguration GetGameConfiguration(CommandList commandList);
   }
}
