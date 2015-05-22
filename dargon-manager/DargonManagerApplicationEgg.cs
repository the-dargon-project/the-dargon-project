using Castle.DynamicProxy;
using Dargon.Nest.Egg;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dargon.Daemon;

namespace Dargon.Manager {
   public class DargonManagerApplicationEgg : INestApplicationEgg {
      public NestResult Start(IEggParameters parameters) {
         ICollectionFactory collectionFactory = new CollectionFactory();
         ProxyGenerator proxyGenerator = new ProxyGenerator();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IPofContext pofContext = new ClientPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         var clusteringConfiguration = new ClientClusteringConfiguration();
         var serviceClientFactory = new ServiceClientFactory(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         var serviceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         Thread.Sleep(1000);
         Console.WriteLine("!A");
         var daemonService = serviceClient.GetService<DaemonService>();
         Console.WriteLine("!B");
         daemonService.Shutdown();
         //Console.WriteLine(daemonService.Configuration.AppDataDirectoryPath);
         Console.WriteLine("!C");

         new Thread(
            () => {
               if (Application.Current == null)
                  new Application();

               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                  new DargonManagerApplication(serviceClient).Run();
               }));

               Dispatcher.Run();
               GC.KeepAlive(parameters);
            }).Start();
         return NestResult.Success;
      }

      public NestResult Shutdown() {
         throw new NotImplementedException();
      }
   }
}
