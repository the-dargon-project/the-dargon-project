using Dargon.InjectedModule;
using Dargon.LeagueOfLegends;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.Processes.Injection;
using Dargon.Processes.Watching;
using Dargon.Transport;
using Dargon.Tray;
using ItzWarty;
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
      private readonly IProcessProxy processProxy;
      private readonly ProcessInjector processInjector;
      private readonly ProcessDiscoveryMethodFactory processDiscoveryMethodFactory;
      private readonly IProcessDiscoveryMethod processDiscoveryMethod;
      private readonly ProcessWatcher processWatcher;
      private readonly TrayService trayService;
      private readonly ProcessInjectionConfiguration processInjectionConfiguration;
      private readonly ProcessInjectionService processInjectionService;
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly ModificationImportService modificationImportService;
      private readonly IDtpNodeFactory dtpNodeFactory;
      private readonly ISessionFactory sessionFactory;
      private readonly InjectedModuleServiceConfiguration injectedModuleServiceConfiguration;
      private readonly InjectedModuleService injectedModuleService;
      private readonly LeagueGameServiceImpl leagueGameServiceImpl;
      private readonly CountdownEvent shutdownSignal = new CountdownEvent(1);
      private bool isShutdownSignalled = false;

      public event EventHandler BeginShutdown;

      public DaemonServiceImpl()
      {
         InitializeLogging();

         if (configuration.IsDebugCompilation) {
            logger.Error("COMPILED IN DEBUG MODE");
         }

         logger.Info("Initializing Daemon");

         serviceLocator.RegisterService(typeof(DaemonService), this);

         processProxy = new ProcessProxy();
         processInjector = new ProcessInjector();
         processDiscoveryMethodFactory = new ProcessDiscoveryMethodFactory();
         processDiscoveryMethod = processDiscoveryMethodFactory.CreateOptimalProcessDiscoveryMethod();
         processWatcher = new ProcessWatcher(processProxy, processDiscoveryMethod);
         trayService = new TrayServiceImpl(serviceLocator, this);
         processInjectionConfiguration = new ProcessInjectionConfiguration(100, 200);
         processInjectionService = new ProcessInjectionServiceImpl(serviceLocator, processInjector, processInjectionConfiguration);
         processWatcherService = new ProcessWatcherServiceImpl(serviceLocator, processProxy, processWatcher).With(s => s.Initialize());
         modificationRepositoryService = new ModificationRepositoryServiceImpl(serviceLocator);
         modificationImportService = new ModificationImportServiceImpl(serviceLocator);
         dtpNodeFactory = new DefaultDtpNodeFactory();
         sessionFactory = new SessionFactory(dtpNodeFactory);
         injectedModuleServiceConfiguration = new InjectedModuleServiceConfiguration();
         injectedModuleService = new InjectedModuleServiceImpl(serviceLocator, processInjectionService, sessionFactory, injectedModuleServiceConfiguration);

         leagueGameServiceImpl = new LeagueGameServiceImpl(this, processProxy, injectedModuleService, processWatcherService, modificationRepositoryService, modificationImportService);
      }

      public IDaemonConfiguration Configuration { get { return configuration; } }

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