using Dargon.Daemon;
using Dargon.FinalFantasyXIII.Lifecycle;
using Dargon.FinalFantasyXIII.Processes;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.Processes.Watching;
using ItzWarty;
using ItzWarty.Processes;
using NLog;

namespace Dargon.FinalFantasyXIII
{
   public class FFXIIIGameServiceImpl : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly DaemonService daemonService;
      private readonly IProcessProxy processProxy;
      private readonly ProcessWatcherService processWatcherService;
      private readonly FFXIIIProcessWatcherServiceImpl ffxiiiProcessWatcherService;
      private readonly FFXIIIInjectedModuleConfigurationFactoryImpl ffxiiiInjectedModuleConfigurationFactory;
      private readonly FFXIIILifecycleServiceImpl ffxiiiLifecycleService;

      public FFXIIIGameServiceImpl(DaemonService daemonService, IProcessProxy processProxy, ProcessWatcherService processWatcherService)
      {
         this.daemonService = daemonService;
         this.processProxy = processProxy;
         this.processWatcherService = processWatcherService;

         this.ffxiiiProcessWatcherService = new FFXIIIProcessWatcherServiceImpl(processWatcherService).With(x => x.Initialize());
         this.ffxiiiInjectedModuleConfigurationFactory = new FFXIIIInjectedModuleConfigurationFactoryImpl();
         this.ffxiiiLifecycleService = new FFXIIILifecycleServiceImpl(ffxiiiProcessWatcherService, ffxiiiInjectedModuleConfigurationFactory).With(x => x.Initialize());
      }
   }
}
