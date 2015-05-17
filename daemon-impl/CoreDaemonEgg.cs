using Castle.DynamicProxy;
using Dargon.FinalFantasyXIII;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.LeagueOfLegends;
using Dargon.Management;
using Dargon.Management.Server;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Processes.Injection;
using Dargon.Processes.Watching;
using Dargon.Services;
using Dargon.Services.Clustering.Host;
using Dargon.Transport;
using Dargon.Tray;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Processes;
using ItzWarty.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Threading;
using Dargon.Services.Server;

namespace Dargon.Daemon {
   public class CoreDaemonApplicationEgg : INestApplicationEgg {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int kDaemonManagementPort = 21000;
      private DaemonServiceImpl daemonService;
      private Thread mainThread;

      public NestResult Start(IEggParameters parameters) {
         if (!logger.IsErrorEnabled) {
            InitializeLogging();
         }

#if DEBUG
         logger.Error("COMPILED IN DEBUG MODE");
#endif

         daemonService = CreateDaemonCore();
         mainThread = new Thread(() => {
            daemonService.Run();
         }).With(t => t.Start());

         return NestResult.Success;
      }

      public NestResult Shutdown() {
         daemonService.Shutdown();
         mainThread.Join();

         return NestResult.Failure;
      }

      private void InitializeLogging() {
         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget();
         Target consoleTarget = new ColoredConsoleTarget();

#if !DEBUG
         debuggerTarget = new AsyncTargetWrapper(debuggerTarget);
         consoleTarget = new AsyncTargetWrapper(consoleTarget);
#else
         AsyncTargetWrapper a; // Placeholder for optimizing imports
#endif

         config.AddTarget("debugger", debuggerTarget);
         config.AddTarget("console", consoleTarget);

         var debuggerRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(debuggerRule);

         var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
         config.LoggingRules.Add(consoleRule);

         LogManager.Configuration = config;
      }

      private static DaemonServiceImpl CreateDaemonCore() {
         // construct libwarty dependencies
         ICollectionFactory collectionFactory = new CollectionFactory();

         // construct libwarty-proxies dependencies
         IStreamFactory streamFactory = new StreamFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IProcessProxy processProxy = new ProcessProxy();

         // construct Castle.Core dependencies
         ProxyGenerator proxyGenerator = new ProxyGenerator();

         // construct dargon common Portable Object Format dependencies
         IPofContext pofContext = new ClientPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);

         // construct libdargon.management dependencies
         ITcpEndPoint managementServerEndpoint = networkingProxy.CreateAnyEndPoint(kDaemonManagementPort);
         var managementFactory = new ManagementFactoryImpl(collectionFactory, threadingProxy, networkingProxy, pofContext, pofSerializer);
         var localManagementServer = managementFactory.CreateServer(new ManagementServerConfiguration(managementServerEndpoint));
         
         // construct root Dargon dependencies.
         var configuration = new ClientConfiguration();

         // construct system-state dependencies
         var systemState = new ClientSystemStateImpl(fileSystemProxy, configuration.ConfigurationDirectoryPath);
         localManagementServer.RegisterInstance(new ClientSystemStateMob(systemState));

         // construct libdsp dependencies
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);
         IServiceClientFactory serviceClientFactory = new ServiceClientFactory(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);

         // construct libdsp local service node
         IClusteringConfiguration clusteringConfiguration = new ClientClusteringConfiguration();
         IServiceClient localServiceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);

         // construct Dargon Daemon dependencies
         var core = new DaemonServiceImpl(configuration);
         DaemonService daemonService = core;
         localServiceClient.RegisterService(daemonService, typeof(DaemonService));
         localManagementServer.RegisterInstance(new DaemonServiceMob(core));

         // construct miscellanious common Dargon dependencies
         TemporaryFileService temporaryFileService = new TemporaryFileServiceImpl(configuration);
         IDtpNodeFactory dtpNodeFactory = new DefaultDtpNodeFactory();

         // construct modification and repository dependencies
         IModificationMetadataSerializer modificationMetadataSerializer = new ModificationMetadataSerializer(fileSystemProxy);
         IModificationMetadataFactory modificationMetadataFactory = new ModificationMetadataFactory();
         IBuildConfigurationLoader buildConfigurationLoader = new BuildConfigurationLoader();
         IModificationLoader modificationLoader = new ModificationLoader(modificationMetadataSerializer, buildConfigurationLoader);
         ModificationRepositoryService modificationRepositoryService = new ModificationRepositoryServiceImpl(configuration, fileSystemProxy, modificationLoader, modificationMetadataSerializer, modificationMetadataFactory).With(s => s.Initialize());
         localServiceClient.RegisterService(modificationRepositoryService, typeof(ModificationRepositoryService));

         // construct process watching/injection dependencies
         IProcessInjector processInjector = new ProcessInjector();
         IProcessDiscoveryMethodFactory processDiscoveryMethodFactory = new ProcessDiscoveryMethodFactory();
         IProcessDiscoveryMethod processDiscoveryMethod = processDiscoveryMethodFactory.CreateOptimalProcessDiscoveryMethod();
         IProcessWatcher processWatcher = new ProcessWatcher(processProxy, processDiscoveryMethod);
         TrayService trayService = new TrayServiceImpl(daemonService);
         IProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfiguration(100, 200);
         ProcessInjectionService processInjectionService = new ProcessInjectionServiceImpl(processInjector, processInjectionConfiguration);
         ProcessWatcherService processWatcherService = new ProcessWatcherServiceImpl(processProxy, processWatcher).With(s => s.Initialize());
         ISessionFactory sessionFactory = new SessionFactory(dtpNodeFactory);
         IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration = new InjectedModuleServiceConfiguration();
         InjectedModuleService injectedModuleService = new InjectedModuleServiceImpl(processInjectionService, sessionFactory, injectedModuleServiceConfiguration).With(x => x.Initialize());
         localServiceClient.RegisterService(injectedModuleService, typeof(InjectedModuleService));
         localServiceClient.RegisterService(processInjectionService, typeof(ProcessInjectionService));

         // construct additional Dargon dependencies
         IGameHandler leagueGameServiceImpl = new LeagueGameServiceImpl(threadingProxy, fileSystemProxy, localManagementServer, daemonService, temporaryFileService, processProxy, injectedModuleService, processWatcherService, modificationRepositoryService);
         IGameHandler ffxiiiGameServiceImpl = new FFXIIIGameServiceImpl(daemonService, processProxy, injectedModuleService, processWatcherService);
         return core;
      }
   }
}
