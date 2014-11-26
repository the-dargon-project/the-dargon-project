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
using ItzWarty.IO;
using ItzWarty.Processes;
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

         var core = new DaemonServiceImpl(configuration);
         DaemonService daemonService = core;
         TemporaryFileService temporaryFileService = new TemporaryFileServiceImpl(configuration);
         IStreamFactory streamFactory = new StreamFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         IProcessProxy processProxy = new ProcessProxy();
         IProcessInjector processInjector = new ProcessInjector();
         IProcessDiscoveryMethodFactory processDiscoveryMethodFactory = new ProcessDiscoveryMethodFactory();
         IProcessDiscoveryMethod processDiscoveryMethod = processDiscoveryMethodFactory.CreateOptimalProcessDiscoveryMethod();
         IProcessWatcher processWatcher = new ProcessWatcher(processProxy, processDiscoveryMethod);
         TrayService trayService = new TrayServiceImpl(daemonService);
         IProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfiguration(100, 200);
         ProcessInjectionService processInjectionService = new ProcessInjectionServiceImpl(processInjector, processInjectionConfiguration);
         ProcessWatcherService processWatcherService = new ProcessWatcherServiceImpl(processProxy, processWatcher).With(s => s.Initialize());
         IModificationMetadataSerializer modificationMetadataSerializer = new ModificationMetadataSerializer(fileSystemProxy);
         IModificationMetadataFactory modificationMetadataFactory = new ModificationMetadataFactory();
         IBuildConfigurationLoader buildConfigurationLoader = new BuildConfigurationLoader();
         IModificationLoader modificationLoader = new ModificationLoader(modificationMetadataSerializer, buildConfigurationLoader);
         ModificationRepositoryService modificationRepositoryService = new ModificationRepositoryServiceImpl(configuration, fileSystemProxy, modificationLoader, modificationMetadataSerializer, modificationMetadataFactory).With(s => s.Initialize());
         IDtpNodeFactory dtpNodeFactory = new DefaultDtpNodeFactory();
         ISessionFactory sessionFactory = new SessionFactory(dtpNodeFactory);
         IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration = new InjectedModuleServiceConfiguration();
         InjectedModuleService injectedModuleService = new InjectedModuleServiceImpl(processInjectionService, sessionFactory, injectedModuleServiceConfiguration).With(x => x.Initialize());
         IGameHandler leagueGameServiceImpl = new LeagueGameServiceImpl(daemonService, temporaryFileService, processProxy, injectedModuleService, processWatcherService, modificationRepositoryService);
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
