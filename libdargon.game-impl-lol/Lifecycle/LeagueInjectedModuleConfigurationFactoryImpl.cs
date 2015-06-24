using Dargon.InjectedModule.Commands;
using Dargon.Trinkets.Components;
using ItzWarty.Collections;
using SCG = System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Lifecycle {
   public class LeagueInjectedModuleConfigurationFactoryImpl : LeagueInjectedModuleConfigurationFactory
   {
      public SCG.IReadOnlyCollection<TrinketComponent> GetPreclientConfigurationComponents()
      {
         var suspendedProcessNames = new HashSet<string> { "LolClient.exe", "League of Legends.exe" };
         return new SCG.List<TrinketComponent> {
            new DebugComponent(),
            new NameComponent("LAUNCHER"),
            new ProcessSuspensionComponent(suspendedProcessNames),
            new VerboseLoggerComponent()
         };
      }

      public SCG.IReadOnlyCollection<TrinketComponent> GetClientConfigurationComponents(ICommandList commandList) {
         return new SCG.List<TrinketComponent> {
            new DebugComponent(),
            new NameComponent("CLIENT"),
            new CommandListComponent(),
            new FilesystemComponent(true),
            new VerboseLoggerComponent()
         };
      }

      public SCG.IReadOnlyCollection<TrinketComponent> GetGameConfigurationComponents(ICommandList commandList) {
         return new SCG.List<TrinketComponent> {
            new DebugComponent(),
            new NameComponent("GAME"),
            new CommandListComponent(),
            new FilesystemComponent(true),
            new VerboseLoggerComponent()
         };
      }
   }
}