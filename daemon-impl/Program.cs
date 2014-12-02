using Castle.DynamicProxy;
using Dargon.FinalFantasyXIII;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.LeagueOfLegends;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.PortableObjects;
using Dargon.Processes.Injection;
using Dargon.Processes.Watching;
using Dargon.Services;
using Dargon.Services.Server;
using Dargon.Services.Server.Phases;
using Dargon.Services.Server.Sessions;
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
         IPofContext pofContext = new CommonPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         
         // construct libdsp dependencies
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(collectionFactory, pofSerializer);
         IPhaseFactory phaseFactory = new PhaseFactory(collectionFactory, threadingProxy, networkingProxy, hostSessionFactory, pofSerializer);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, networkingProxy, phaseFactory);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         IServiceNodeFactory serviceNodeFactory = new ServiceNodeFactory(connectorFactory, serviceContextFactory, collectionFactory);

         // construct libdsp local service node
         IServiceConfiguration serviceConfiguration = new DargonServiceConfiguration();
         IServiceNode localServiceNode = serviceNodeFactory.CreateOrJoin(serviceConfiguration);
         
         // construct Dargon Daemon dependencies
         DaemonService daemonService = core;
         localServiceNode.RegisterService(daemonService, typeof(DaemonService));

         // construct miscellanious common Dargon dependencies
         TemporaryFileService temporaryFileService = new TemporaryFileServiceImpl(configuration);
         IDtpNodeFactory dtpNodeFactory = new DefaultDtpNodeFactory();

         // construct modification and repository dependencies
         IModificationMetadataSerializer modificationMetadataSerializer = new ModificationMetadataSerializer(fileSystemProxy);
         IModificationMetadataFactory modificationMetadataFactory = new ModificationMetadataFactory();
         IBuildConfigurationLoader buildConfigurationLoader = new BuildConfigurationLoader();
         IModificationLoader modificationLoader = new ModificationLoader(modificationMetadataSerializer, buildConfigurationLoader);
         ModificationRepositoryService modificationRepositoryService = new ModificationRepositoryServiceImpl(configuration, fileSystemProxy, modificationLoader, modificationMetadataSerializer, modificationMetadataFactory).With(s => s.Initialize());
         localServiceNode.RegisterService(modificationRepositoryService, typeof(ModificationRepositoryService));

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
         localServiceNode.RegisterService(injectedModuleService, typeof(InjectedModuleService));
         localServiceNode.RegisterService(processInjectionService, typeof(ProcessInjectionService));

         // construct additional Dargon dependencies
         IGameHandler leagueGameServiceImpl = new LeagueGameServiceImpl(threadingProxy, daemonService, temporaryFileService, processProxy, injectedModuleService, processWatcherService, modificationRepositoryService);
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
