using System.Collections.Generic;
using Dargon.Daemon;
using Dargon.FinalFantasyXIII.Lifecycle;
using Dargon.FinalFantasyXIII.Processes;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.LeagueOfLegends.Lifecycle;
using Dargon.Processes.Watching;
using ItzWarty;
using NLog;

namespace Dargon.FinalFantasyXIII
{
   public class FFXIIIGameServiceImpl : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly DaemonService daemonService;
      private readonly IProcessProxy processProxy;
      private readonly InjectedModuleService injectedModuleService;
      private readonly ProcessWatcherService processWatcherService;
      private readonly FFXIIIProcessWatcherServiceImpl ffxiiiProcessWatcherService;
      private readonly FFXIIIInjectedModuleConfigurationFactory ffxiiiInjectedModuleConfigurationFactory;
      private readonly FFXIIILifecycleServiceImpl ffxiiiLifecycleService;

      public FFXIIIGameServiceImpl(DaemonService daemonService, IProcessProxy processProxy, InjectedModuleService injectedModuleService, ProcessWatcherService processWatcherService)
      {
         this.daemonService = daemonService;
         this.processProxy = processProxy;
         this.injectedModuleService = injectedModuleService;
         this.processWatcherService = processWatcherService;

         this.ffxiiiProcessWatcherService = new FFXIIIProcessWatcherServiceImpl(processWatcherService).With(x => x.Initialize());
         this.ffxiiiInjectedModuleConfigurationFactory = new FFXIIIInjectedModuleConfigurationFactory();
         this.ffxiiiLifecycleService = new FFXIIILifecycleServiceImpl(injectedModuleService, ffxiiiProcessWatcherService, ffxiiiInjectedModuleConfigurationFactory).With(x => x.Initialize());
      }
   }
}
