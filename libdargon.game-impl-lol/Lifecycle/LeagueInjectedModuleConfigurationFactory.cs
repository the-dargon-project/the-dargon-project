using Dargon.InjectedModule.Commands;
using Dargon.Trinkets.Components;
using SCG = System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Lifecycle {
   public interface LeagueInjectedModuleConfigurationFactory
   {
      SCG.IReadOnlyCollection<TrinketComponent> GetPreclientConfigurationComponents();
      SCG.IReadOnlyCollection<TrinketComponent> GetClientConfigurationComponents(ICommandList commandList);
      SCG.IReadOnlyCollection<TrinketComponent> GetGameConfigurationComponents(ICommandList commandList);
   }
}
