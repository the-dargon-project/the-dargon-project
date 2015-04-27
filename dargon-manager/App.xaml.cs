using System;
using System.Threading;
using System.Windows;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services.Client;
using DargonManager;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;
using Dargon.Services;
using Dargon.Services.Clustering.Host;
using Dargon.Services.Server;

namespace Dargon.Manager {
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application {
      private void Application_Startup(object sender, StartupEventArgs e) {
         Console.WriteLine("Entered Application_Startup");

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
         IHostSessionFactory hostSessionFactory = new HostSessionFactory(threadingProxy, collectionFactory, pofSerializer, pofStreamsFactory);
         InvokableServiceContextFactory invokableServiceContextFactory = new InvokableServiceContextFactoryImpl(collectionFactory);
         var serviceClientFactory = new ServiceClientFactory(proxyGenerator, collectionFactory, threadingProxy, networkingProxy, pofStreamsFactory, hostSessionFactory, invokableServiceContextFactory);
         var reconnectAttempts = 10;
         var reconnectDelay = 1000;
         var serviceClient = TryConnectToEndpoint(reconnectAttempts, reconnectDelay, serviceClientFactory, clusteringConfiguration);
         if (serviceClient == null) {
            Console.Error.WriteLine("Failed to connect to endpoint.");
         } else {
            new DargonManagerApplication(serviceClient).Run();
         }
      }

      private static IServiceClient TryConnectToEndpoint(int reconnectAttempts, int reconnectDelay, ServiceClientFactory serviceClientFactory, ClientClusteringConfiguration clusteringConfiguration) {
         IServiceClient serviceClient = null;
         for (var i = 0; i < reconnectAttempts && serviceClient == null; i++) {
            try {
               serviceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
            } catch (Exception e) {
               Console.WriteLine(e);
               if (i == 0) {
                  Console.Write("Connecting to local endpoint on port " + clusteringConfiguration.Port + ".");
               } else if (i > 0) {
                  Console.Write(".");
               }
               Thread.Sleep(reconnectDelay);
            }
            if (serviceClient != null && i > 0) {
               Console.WriteLine();
            }
         }
         return serviceClient;
      }
   }
}