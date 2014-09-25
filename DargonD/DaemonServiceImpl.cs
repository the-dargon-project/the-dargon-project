using Dargon.InjectedModule;
using Dargon.LeagueOfLegends;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.Processes.Injection;
using Dargon.Processes.Watching;
using Dargon.Tray;
using ItzWarty.Services;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Threading;

namespace Dargon.Daemon
{
   public class DaemonServiceImpl : DaemonService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly DaemonConfiguration configuration = new DaemonConfiguration();
      private readonly ServiceLocator serviceLocator = new ServiceLocator();
      private readonly TrayService trayService;
      private readonly ProcessInjectionService processInjectionService;
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly ModificationImportService modificationImportService;
      private readonly InjectedModuleService injectedModuleService;
      private readonly LeagueGameServiceImpl leagueGameServiceImpl;
      private readonly CountdownEvent shutdownSignal = new CountdownEvent(1);
      private bool isShutdownSignalled = false;
      public event EventHandler BeginShutdown;

      public DaemonServiceImpl()
      {
         InitializeLogging();
         logger.Info("Initializing Daemon");

         serviceLocator.RegisterService(typeof(DaemonService), this);

         trayService = new TrayServiceImpl(serviceLocator, this);
         processInjectionService = new ProcessInjectionServiceImpl(serviceLocator);
         processWatcherService = new ProcessWatcherServiceImpl(serviceLocator);
         modificationRepositoryService = new ModificationRepositoryServiceImpl(serviceLocator);
         modificationImportService = new ModificationImportServiceImpl(serviceLocator);

         injectedModuleService = new InjectedModuleServiceImpl(serviceLocator, processInjectionService);

         leagueGameServiceImpl = new LeagueGameServiceImpl(this, processWatcherService, modificationRepositoryService, modificationImportService);
      }

      private void InitializeLogging()
      {
         var config = new LoggingConfiguration();
         var debuggerTarget = new DebuggerTarget();
         config.AddTarget("debugger", debuggerTarget);

         var rule2 = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(rule2);
         LogManager.Configuration = config;
      }

      public void Run() { shutdownSignal.Wait(); }

      public bool IsShutdownSignalled { get { return isShutdownSignalled; } }

      public void Shutdown()
      {
         isShutdownSignalled = true;
         var capture = BeginShutdown;
         if (capture != null)
            capture(this, new EventArgs());
         shutdownSignal.Signal();
      }
   }
}