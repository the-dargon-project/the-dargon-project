using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Castle.DynamicProxy;
using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Processes.Injection;
using Dargon.Services;
using Dargon.Services.Clustering;
using Dargon.Services.Messaging;
using Dargon.Transport;
using Dargon.Trinkets.Transport;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Processes;
using ItzWarty.Threading;

namespace Dargon.Trinkets.Proxy {
   public class TrinketProxyEgg : INestApplicationEgg {
      private readonly StreamFactory streamFactory;
      private readonly IProcessProxy processProxy;
      private readonly PofSerializer pofSerializer;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ProcessInjectionService processInjectionService;
      private readonly TrinketInternalUtilities trinketInternalUtilities;
      private readonly TrinketDtpServerFactoryImpl trinketDtpServerFactory;
      private readonly List<object> keepaliveObjects = new List<object>();
      private TrinketDtpServer trinketDtpServer;
      private IEggHost host;

      public TrinketProxyEgg() {
         streamFactory = new StreamFactory();
         processProxy = new ProcessProxy();
         var pofContext = new PofContext().With(x => {
            x.MergeContext(new DspPofContext());
            x.MergeContext(new TrinketsApiPofContext());
            x.MergeContext(new TrinketsImplPofContext());
         });
         ICollectionFactory collectionFactory = new CollectionFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         IThreadingProxy threadingProxy = new ThreadingProxy(new ThreadingFactory(), new SynchronizationFactory());
         var dnsProxy = new DnsProxy();
         INetworkingProxy networkingProxy = new NetworkingProxy(new SocketFactory(new TcpEndPointFactory(dnsProxy), new NetworkingInternalFactory(threadingProxy, streamFactory)), new TcpEndPointFactory(dnsProxy));
         pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         ProxyGenerator proxyGenerator = new ProxyGenerator();
         var serviceClientFactory = new ServiceClientFactoryImpl(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);

         // construct libdsp local service node
         ClusteringConfiguration clusteringConfiguration = new ClientClusteringConfiguration();
         ServiceClient localServiceClient = serviceClientFactory.Construct(clusteringConfiguration);
         keepaliveObjects.Add(localServiceClient);

         temporaryFileService = localServiceClient.GetService<TemporaryFileService>();

         var processInjector = new ProcessInjectorImpl();
         ProcessInjectionConfiguration processInjectionConfiguration = new ProcessInjectionConfigurationImpl(injectionAttempts: 10, injectionAttemptsDelay: 200);
         processInjectionService = new ProcessInjectionServiceImpl(processInjector, processInjectionConfiguration);
         IDtpNodeFactory transportNodeFactory = new DefaultDtpNodeFactory();
         BootstrapConfigurationGenerator bootstrapConfigurationGenerator = new BootstrapConfigurationGeneratorImpl();
         trinketInternalUtilities = new TrinketInternalUtilitiesImpl(fileSystemProxy);
         trinketDtpServerFactory = new TrinketDtpServerFactoryImpl(streamFactory, transportNodeFactory, bootstrapConfigurationGenerator);
      }

      public NestResult Start(IEggParameters parameters) {
         host = parameters.Host;
         var configuration = pofSerializer.Deserialize<TrinketStartupConfiguration>(streamFactory.CreateMemoryStream(parameters.Arguments).Reader);
         trinketDtpServer = trinketDtpServerFactory.Create(configuration);
         var trinketBridge = new TrinketBridgeImpl(temporaryFileService, processInjectionService, trinketInternalUtilities, configuration, trinketDtpServer);
         keepaliveObjects.Add(trinketBridge);
         var injectionSuccessful = trinketBridge.Initialize();
         if (injectionSuccessful) {
            var process = processProxy.GetProcessById(configuration.TargetProcessId);
            process.Exited += (o, s) => {
               Shutdown(ShutdownReason.None);
            };
            process.EnableRaisingEvents = true;
         }

         return injectionSuccessful ? NestResult.Success : NestResult.Failure;
      }

      public NestResult Shutdown(ShutdownReason reason) {
         trinketDtpServer.Dispose();
         host.Shutdown();
         return NestResult.Success;
      }
   }
}
