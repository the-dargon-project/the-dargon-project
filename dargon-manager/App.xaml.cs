using System;
using System.Threading;
using System.Windows;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.Client;
using DargonManager;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
       private void Application_Startup(object sender, StartupEventArgs e) {
         Console.WriteLine("Entered Application_Startup");

         ICollectionFactory collectionFactory = new CollectionFactory();
         ProxyGenerator proxyGenerator = new ProxyGenerator();
         IServiceProxyFactory serviceProxyFactory = new ServiceProxyFactory(proxyGenerator);
         IServiceContextFactory serviceContextFactory = new ServiceContextFactory(collectionFactory);
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory networkingProxy = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         IInvocationStateFactory invocationStateFactory = new InvocationStateFactory(threadingProxy);
         IPofContext pofContext = new ClientPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, networkingProxy, invocationStateFactory, pofSerializer);

         var serviceConfiguration = new ClientServiceConfiguration();
         var serviceClientFactory = new ServiceClientFactory(collectionFactory, serviceProxyFactory, serviceContextFactory, connectorFactory);
         var localEndPoint = tcpEndPointFactory.CreateLoopbackEndPoint(serviceConfiguration.Port);
         var reconnectAttempts = 10;
         var reconnectDelay = 1000;
         var serviceClient = TryConnectToEndpoint(reconnectAttempts, reconnectDelay, serviceClientFactory, localEndPoint, serviceConfiguration);
         if (serviceClient == null) {
            Console.Error.WriteLine("Failed to connect to endpoint.");
         } else {
            new DargonManagerApplication(serviceClient).Run();
         }
      }

      private static IServiceClient TryConnectToEndpoint(int reconnectAttempts, int reconnectDelay, ServiceClientFactory serviceClientFactory, ITcpEndPoint endpoint, ClientServiceConfiguration serviceConfiguration) {
         IServiceClient serviceClient = null;
         for (var i = 0; i < reconnectAttempts && serviceClient == null; i++) {
            try {
               serviceClient = serviceClientFactory.Create(endpoint);
            } catch (Exception e) {
               Console.WriteLine(e);
               if (i == 0) {
                  Console.Write("Connecting to local endpoint on port " + serviceConfiguration.Port + ".");
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
