using Dargon.FinalFantasyXIII;
using Dargon.Game;
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

namespace Dargon.Daemon
{
   public static class Program
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public static void Main(string[] args) { 
         InitializeLogging();

         var configuration = new DaemonConfiguration();
         if (configuration.IsDebugCompilation) {
            logger.Error("COMPILED IN DEBUG MODE");
         }

         var serviceLocator = new ServiceLocator();
         var core = new DaemonServiceImpl(serviceLocator, configuration);
         DaemonService daemonService = core;
         TemporaryFileService temporaryFileService = new TemporaryFileServiceImpl(configuration);
         IFileSystemProxy fileSystemProxy = new FileSystemProxy();
         IProcessProxy processProxy = new ProcessProxy();
         IProcessInjector processInjector = new ProcessInjector();
         IProcessDiscoveryMethodFactory processDiscoveryMethodFactory = new ProcessDiscoveryMethodFactory();
         IProcessDiscoveryMethod processDiscoveryMethod = processDiscoveryMethodFactory.CreateOptimalProcessDiscoveryMethod();
         IProcessWatcher processWatcher = new ProcessWatcher(processProxy, processDiscoveryMethod);
         TrayService trayService = new TrayServiceImpl(serviceLocator, daemonService);
         IProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfiguration(100, 200);
         ProcessInjectionService processInjectionService = new ProcessInjectionServiceImpl(serviceLocator, processInjector, processInjectionConfiguration);
         ProcessWatcherService processWatcherService = new ProcessWatcherServiceImpl(serviceLocator, processProxy, processWatcher).With(s => s.Initialize());
         ModificationRepositoryService modificationRepositoryService = new ModificationRepositoryServiceImpl(serviceLocator);
         ModificationImportService modificationImportService = new ModificationImportServiceImpl(serviceLocator);
         IDtpNodeFactory dtpNodeFactory = new DefaultDtpNodeFactory();
         ISessionFactory sessionFactory = new SessionFactory(dtpNodeFactory);
         IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration = new InjectedModuleServiceConfiguration();
         InjectedModuleService injectedModuleService = new InjectedModuleServiceImpl(serviceLocator, processInjectionService, sessionFactory, injectedModuleServiceConfiguration).With(x => x.Initialize());
         IGameHandler leagueGameServiceImpl = new LeagueGameServiceImpl(daemonService, temporaryFileService, processProxy, injectedModuleService, processWatcherService, modificationRepositoryService, modificationImportService);
         IGameHandler ffxiiiGameServiceImpl = new FFXIIIGameServiceImpl(daemonService, processProxy, injectedModuleService, processWatcherService);
         core.Run();
      }

      private static void InitializeLogging()
      {
         var config = new LoggingConfiguration();
         var debuggerTarget = new DebuggerTarget();
         config.AddTarget("debugger", debuggerTarget);

         var rule2 = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(rule2);
         LogManager.Configuration = config;
      }
   }
}
