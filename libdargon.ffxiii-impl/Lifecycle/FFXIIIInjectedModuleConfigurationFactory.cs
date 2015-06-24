using Dargon.Trinkets.Components;
using SCG = System.Collections.Generic;

namespace Dargon.FinalFantasyXIII.Lifecycle {
   public interface FFXIIIInjectedModuleConfigurationFactory 
   {
      SCG.IReadOnlyCollection<TrinketComponent> GetLauncherConfigurationComponents();
      SCG.IReadOnlyCollection<TrinketComponent> GetGameConfigurationComponents();
   }
}
