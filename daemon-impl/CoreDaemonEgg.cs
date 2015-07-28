using Castle.DynamicProxy;
using Dargon.FinalFantasyXIII;
using Dargon.Game;
using Dargon.LeagueOfLegends;
using Dargon.Management;
using Dargon.Management.Server;
using Dargon.Modifications;
using Dargon.Nest.Egg;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Processes.Watching;
using Dargon.Services;
using Dargon.Transport;
using Dargon.Tray;
using Dargon.Trinkets;
using Dargon.Trinkets.Commands;
using Dargon.Trinkets.Components;
using Dargon.VirtualFileMaps;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dargon.Trinkets.Spawner;

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

         List<object> keepalive;
         daemonService = CreateDaemonCore(parameters, out keepalive);
         mainThread = new Thread(() => {
            daemonService.Run();
            GC.KeepAlive(keepalive);
            parameters.Host.Shutdown();
         }) { IsBackground = false }.With(t => t.Start());

         return NestResult.Success;
      }

      public NestResult Shutdown() {
         daemonService.Shutdown();
         mainThread.Join();

         return NestResult.Success;
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

      private static DaemonServiceImpl CreateDaemonCore(IEggParameters parameters, out List<object> keepalive) {
         keepalive = new List<object>();
         keepalive.Add(parameters);

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
         keepalive.Add(localManagementServer);

         // construct root Dargon dependencies.
         var clientConfiguration = new ClientConfiguration();

         // construct system-state dependencies
         var systemState = new ClientSystemStateImpl(fileSystemProxy, clientConfiguration.ConfigurationDirectoryPath);
         localManagementServer.RegisterInstance(new ClientSystemStateMob(systemState));

         // construct libdsp dependencies
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);
         IServiceClientFactory serviceClientFactory = new ServiceClientFactory(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);

         // construct libdsp local service node
         IClusteringConfiguration clusteringConfiguration = new ClientClusteringConfiguration();
         IServiceClient localServiceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         keepalive.Add(localServiceClient);

         // construct Dargon Daemon dependencies
         var core = new DaemonServiceImpl(clientConfiguration);
         DaemonService daemonService = core;
         localServiceClient.RegisterService(daemonService, typeof(DaemonService));
         localManagementServer.RegisterInstance(new DaemonServiceMob(core));

         // construct miscellanious common Dargon dependencies
         TemporaryFileService temporaryFileService = new TemporaryFileServiceImpl(clientConfiguration, fileSystemProxy).With(x => x.Initialize());
         localServiceClient.RegisterService(temporaryFileService, typeof(TemporaryFileService));
         IDtpNodeFactory dtpNodeFactory = new DefaultDtpNodeFactory();

         // get the exeggutor service
         ExeggutorService exeggutorService = localServiceClient.GetService<ExeggutorService>();

         // construct modification and repository dependencies
         var modificationComponentFactory = new ModificationComponentFactory(fileSystemProxy, pofContext, new SlotSourceFactoryImpl(), pofSerializer);
         ModificationLoader modificationLoader = new ModificationLoaderImpl(clientConfiguration.RepositoriesDirectoryPath, modificationComponentFactory);

         // construct process watching/injection dependencies
//         IProcessInjector processInjector = new ProcessInjector();
         IProcessDiscoveryMethodFactory processDiscoveryMethodFactory = new ProcessDiscoveryMethodFactory();
         IProcessDiscoveryMethod processDiscoveryMethod = processDiscoveryMethodFactory.CreateOptimalProcessDiscoveryMethod();
         IProcessWatcher processWatcher = new ProcessWatcher(processProxy, processDiscoveryMethod);
         TrayService trayService = new TrayServiceImpl(daemonService);
//         IProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfiguration(100, 200);
//         ProcessInjectionService processInjectionService = new ProcessInjectionServiceImpl(processInjector, processInjectionConfiguration);
         ProcessWatcherService processWatcherService = new ProcessWatcherServiceImpl(processProxy, processWatcher).With(s => s.Initialize());
         //         ISessionFactory sessionFactory = new SessionFactory(dtpNodeFactory);
         //         IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration = new InjectedModuleServiceConfiguration();
         //         InjectedModuleService injectedModuleService = new InjectedModuleServiceImpl(processInjectionService, sessionFactory, injectedModuleServiceConfiguration).With(x => x.Initialize());
         //         localServiceClient.RegisterService(injectedModuleService, typeof(InjectedModuleService));
         //         localServiceClient.RegisterService(processInjectionService, typeof(ProcessInjectionService));

         CommandFactory commandFactory = new CommandFactoryImpl();
         TrinketSpawner trinketSpawner = new TrinketSpawnerImpl(streamFactory, pofSerializer, exeggutorService);

//         var sectors = new SectorCollection();
//         sectors.AssignSector(new SectorRange(0, 26), new FileSector("C:/dummy-files/lowercase.txt", 0, 26));
//         sectors.AssignSector(new SectorRange(28, 54), new FileSector("C:/dummy-files/uppercase.txt", 0, 26));
//         sectors.AssignSector(new SectorRange(54, 64), new FileSector("C:/dummy-files/digits.txt", 0, 10));
//         sectors.AssignSector(new SectorRange(65, 68), new FileSector("C:/dummy-files/digits.txt", 3, 6));
//         var vfm = new VirtualFile(sectors);
//         var vfmSerializer = new SectorCollectionSerializer();
//         using (var fs = fileSystemProxy.OpenFile("C:/dummy-files/z.vfm", FileMode.Create, FileAccess.Write))
//         using (var writer = fs.Writer) {
//            vfmSerializer.Serialize(sectors, writer.__Writer);
//         }
//
//         var targetProcess = processWatcher.FindProcess(x => x.ProcessName.Contains("notepad++")).First();
//         logger.Info("TARGET PID " + targetProcess.Id);
//         trinketSpawner.SpawnTrinket(
//            targetProcess, 
//            new TrinketSpawnConfigurationImpl {
//               Name = "npp",
//               IsFileSystemOverridingEnabled = true,
//               IsFileSystemHookingEnabled = true,
//               IsCommandingEnabled = true,
//               CommandList = new DefaultCommandList(new [] {
//                  commandFactory.CreateFileRemappingCommand("C:/dummy-files/a.txt", "C:/Users/ItzWarty/.dargon/temp/9b2e6d0cbf57024d96ff646476398f26/0.0.0.235/Archive_3.raf.dat.vfm")
//               })
//            }
//         );
//         logger.Info("######################################");

         // construct additional Dargon dependencies
         IGameHandler leagueGameServiceImpl = new LeagueGameServiceImpl(clientConfiguration, threadingProxy, fileSystemProxy, localManagementServer, localServiceClient, daemonService, temporaryFileService, processProxy, processWatcherService, modificationLoader, trinketSpawner);
         IGameHandler ffxiiiGameServiceImpl = new FFXIIIGameServiceImpl(daemonService, processProxy, processWatcherService);

         return core;
      }
   }
}
