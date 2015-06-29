using Dargon.FinalFantasyXIII.Processes;
using System;

namespace Dargon.FinalFantasyXIII.Lifecycle {
   public class FFXIIILifecycleServiceImpl
   {
      private readonly FFXIIIProcessWatcherServiceImpl ffxiiiProcessWatcherService;
      private readonly FFXIIIInjectedModuleConfigurationFactoryImpl ffxiiiInjectedModuleConfigurationFactory;

      public FFXIIILifecycleServiceImpl(FFXIIIProcessWatcherServiceImpl ffxiiiProcessWatcherService, FFXIIIInjectedModuleConfigurationFactoryImpl ffxiiiInjectedModuleConfigurationFactory)
      {
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
         var configuration = ffxiiiInjectedModuleConfigurationFactory.GetLauncherConfigurationComponents();
         throw new NotImplementedException();
//         injectedModuleService.InjectToProcess(e.ProcessDescriptor.ProcessId, configuration);
      }

      private void HandleGameLaunched(FFXIIIProcessDetectedArgs e)
      {
         var configuration = ffxiiiInjectedModuleConfigurationFactory.GetGameConfigurationComponents();
         throw new NotImplementedException();
         //         injectedModuleService.InjectToProcess(e.ProcessDescriptor.ProcessId, configuration);
      }
   }
}