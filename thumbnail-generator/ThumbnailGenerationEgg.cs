using Dargon.Management;
using Dargon.Management.Server;
using Dargon.Modifications.ThumbnailGenerator;
using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Processes;
using ItzWarty.Threading;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Dargon.ThumbnailGenerator {
   public class ThumbnailGenerationEgg : INestApplicationEgg {
      private const int kDaemonManagementPort = 21003;

      private readonly CollectionFactory collectionFactory;
      private readonly ThreadingProxy threadingProxy;
      private readonly PofSerializer pofSerializer;
      private readonly PofContext pofContext;
      private readonly INetworkingProxy networkingProxy;
      private readonly ThumbnailGeneratorService thumbnailGeneratorService;
      private readonly ManualResetEvent shutdownLatch = new ManualResetEvent(false);
      private readonly List<object> keepalive = new List<object>();
      private IEggParameters parameters;

      public ThumbnailGenerationEgg() {
         collectionFactory = new CollectionFactory();
         var streamFactory = new StreamFactory();
         var processProxy = new ProcessProxy();
         threadingProxy = new ThreadingProxy(new ThreadingFactory(), new SynchronizationFactory());
         pofContext = new PofContext().With(x => {
            x.MergeContext(new ManagementPofContext());
            x.MergeContext(new ThumbnailGeneratorApiPofContext());
         });
         pofSerializer = new PofSerializer(pofContext);
         networkingProxy = new NetworkingProxy(new SocketFactory(new TcpEndPointFactory(new DnsProxy()), new NetworkingInternalFactory(threadingProxy, streamFactory)), new TcpEndPointFactory(new DnsProxy()));
         thumbnailGeneratorService = new ThumbnailGeneratorServiceImpl();
      }

      public NestResult Start(IEggParameters parameters) {
         var thumbnailGenerationParameters = parameters == null ? null : pofSerializer.Deserialize<ThumbnailGenerationParameters>(new MemoryStream(parameters.Arguments));

         this.parameters = parameters;
         if (parameters == null || thumbnailGenerationParameters == null) {
            // construct libdargon.management dependencies
            ITcpEndPoint managementServerEndpoint = networkingProxy.CreateAnyEndPoint(kDaemonManagementPort);
            var managementFactory = new ManagementFactoryImpl(collectionFactory, threadingProxy, networkingProxy, pofContext, pofSerializer);
            var localManagementServer = managementFactory.CreateServer(new ManagementServerConfiguration(managementServerEndpoint));
            keepalive.Add(localManagementServer);
            localManagementServer.RegisterInstance(new ThumbnailGenerationMob(thumbnailGeneratorService));
            shutdownLatch.WaitOne();
         } else {
            thumbnailGeneratorService.GenerateThumbnails(thumbnailGenerationParameters);
         }
         return NestResult.Success;
      }

      public NestResult Shutdown() {
         shutdownLatch.Set();
         parameters?.Host.Shutdown();
         return NestResult.Success;
      }
   }
}
