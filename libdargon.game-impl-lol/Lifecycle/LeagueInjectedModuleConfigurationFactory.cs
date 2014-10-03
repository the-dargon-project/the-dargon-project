using System;
using System.Collections.Generic;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Components;
using Dargon.InjectedModule.Tasks;

namespace Dargon.LeagueOfLegends.Lifecycle
{
   public class LeagueInjectedModuleConfigurationFactory : ILeagueInjectedModuleConfigurationFactory
   {
      public IInjectedModuleConfiguration GetPreclientConfiguration()
      {
         var suspendedProcessNames = new HashSet<string> { "LolClient.exe", "League of Legends.exe" };
         var configurationBuilder = new InjectedModuleConfigurationBuilder();
         configurationBuilder.AddComponent(new DebugConfigurationComponent());
         configurationBuilder.AddComponent(new RoleConfigurationComponent(DimRole.Launcher));
         configurationBuilder.AddComponent(new ProcessSuspensionConfigurationComponent(suspendedProcessNames));
         configurationBuilder.AddComponent(new VerboseLoggerConfigurationComponent());
         return configurationBuilder.Build();
      }

      public IInjectedModuleConfiguration GetClientConfiguration(ITasklist tasklist)
      {
         var configurationBuilder = new InjectedModuleConfigurationBuilder();
         configurationBuilder.AddComponent(new DebugConfigurationComponent());
         configurationBuilder.AddComponent(new RoleConfigurationComponent(DimRole.Client));
         configurationBuilder.AddComponent(new TasklistConfigurationComponent(tasklist));
         configurationBuilder.AddComponent(new FilesystemConfigurationComponent(true));
         configurationBuilder.AddComponent(new VerboseLoggerConfigurationComponent());
         return configurationBuilder.Build();
      }

      public IInjectedModuleConfiguration GetGameConfiguration() { throw new NotImplementedException(); }
   }
}