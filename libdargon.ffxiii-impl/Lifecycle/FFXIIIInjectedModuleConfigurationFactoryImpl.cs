using Dargon.Trinkets.Components;
using SCG = System.Collections.Generic;

namespace Dargon.FinalFantasyXIII.Lifecycle {
   public class FFXIIIInjectedModuleConfigurationFactoryImpl : FFXIIIInjectedModuleConfigurationFactory
   {
      public SCG.IReadOnlyCollection<TrinketComponent> GetLauncherConfigurationComponents()
      {
         var suspendedProcessNames = new ItzWarty.Collections.HashSet<string> { "ffxiiiimg.exe" };
         return new SCG.List<TrinketComponent> {
            new DebugComponent(),
            new NameComponent("LAUNCHER"),
            new ProcessSuspensionComponent(suspendedProcessNames),
            new VerboseLoggerComponent()
         };
      }

      public SCG.IReadOnlyCollection<TrinketComponent> GetGameConfigurationComponents() {
         return new SCG.List<TrinketComponent> {
            new DebugComponent(),
            new NameComponent("GAME"),
            new FilesystemComponent(true),
            new VerboseLoggerComponent()
         };
      }
   }
}