using System;
using Dargon.FinalFantasyXIII.Processes;
using Dargon.InjectedModule;
using Dargon.LeagueOfLegends.Lifecycle;

namespace Dargon.FinalFantasyXIII.Lifecycle
{
   public class FFXIIILifecycleServiceImpl
   {
      private readonly InjectedModuleService injectedModuleService;
      private readonly FFXIIIProcessWatcherServiceImpl ffxiiiProcessWatcherService;
      private readonly FFXIIIInjectedModuleConfigurationFactory ffxiiiInjectedModuleConfigurationFactory;

      public FFXIIILifecycleServiceImpl(InjectedModuleService injectedModuleService, FFXIIIProcessWatcherServiceImpl ffxiiiProcessWatcherService, FFXIIIInjectedModuleConfigurationFactory ffxiiiInjectedModuleConfigurationFactory)
      {
         this.injectedModuleService = injectedModuleService;
         this.ffxiiiProcessWatcherService = ffxiiiProcessWatcherService;
         this.ffxiiiInjectedModuleConfigurationFactory = ffxiiiInjectedModuleConfigurationFactory;
      }

      public void Initialize()
      {
         ffxiiiProcessWatcherService.LauncherLaunched += HandleLauncherLaunched;
         ffxiiiProcessWatcherService.GameLaunched += HandleGameLaunched;
      }

      private void HandleLauncherLaunched(FFXIIIProcessDetectedArgs e)
      {
         var configuration = ffxiiiInjectedModuleConfigurationFactory.GetLauncherConfiguration();
         injectedModuleService.InjectToProcess(e.ProcessDescriptor.ProcessId, configuration);
      }

      private void HandleGameLaunched(FFXIIIProcessDetectedArgs e)
      {
         var configuration = ffxiiiInjectedModuleConfigurationFactory.GetGameConfiguration();
         injectedModuleService.InjectToProcess(e.ProcessDescriptor.ProcessId, configuration);
      }
   }
}