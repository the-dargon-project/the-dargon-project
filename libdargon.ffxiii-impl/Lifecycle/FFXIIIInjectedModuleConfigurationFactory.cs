using System;
using System.Collections.Generic;
using Dargon.FinalFantasyXIII.Lifecycle;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Components;
using Dargon.InjectedModule.Tasks;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class FFXIIIInjectedModuleConfigurationFactory : IFFXIIIInjectedModuleConfigurationFactory
   {
      public IInjectedModuleConfiguration GetLauncherConfiguration()
      {
         var suspendedProcessNames = new HashSet<string> { "ffxiiiimg.exe" };
         var configurationBuilder = new InjectedModuleConfigurationBuilder();
         configurationBuilder.AddComponent(new DebugConfigurationComponent());
         configurationBuilder.AddComponent(new RoleConfigurationComponent(DimRole.Launcher));
         configurationBuilder.AddComponent(new ProcessSuspensionConfigurationComponent(suspendedProcessNames));
         configurationBuilder.AddComponent(new VerboseLoggerConfigurationComponent());
         return configurationBuilder.Build();
      }

      public IInjectedModuleConfiguration GetGameConfiguration()
      {
         var configurationBuilder = new InjectedModuleConfigurationBuilder();
         configurationBuilder.AddComponent(new DebugConfigurationComponent());
         configurationBuilder.AddComponent(new RoleConfigurationComponent(DimRole.Game));
         configurationBuilder.AddComponent(new FilesystemConfigurationComponent(true));
         configurationBuilder.AddComponent(new VerboseLoggerConfigurationComponent());
         return configurationBuilder.Build();
      }
   }
}