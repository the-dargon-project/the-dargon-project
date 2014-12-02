using System;
using Castle.DynamicProxy;
using Dargon.PortableObjects;
using Dargon.Services.Client;
using ItzWarty.Collections;
using ItzWarty.IO;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.CLI {
   public static class Program {
      public static int Main() {
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
         IPofContext pofContext = new CommonPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         IConnectorFactory connectorFactory = new ConnectorFactory(collectionFactory, threadingProxy, networkingProxy, invocationStateFactory, pofSerializer);
         var serviceConfiguration = new DargonServiceConfiguration();
         var serviceClientFactory = new ServiceClientFactory(collectionFactory, serviceProxyFactory, serviceContextFactory, connectorFactory);
         var localEndPoint = tcpEndPointFactory.CreateLoopbackEndPoint(serviceConfiguration.Port);
         var reconnectAttempts = 10;
         var reconnectDelay = 1000;
         var serviceClient = TryConnectToEndpoint(reconnectAttempts, serviceClientFactory, localEndPoint, serviceConfiguration);
         if (serviceClient == null) {
            Console.Error.WriteLine("Failed to connect to endpoint.");
            return 1;
         } else {
            var dispatcher = new DispatcherCommand("registered commands");
            dispatcher.RegisterCommand(new ShutdownCommand(serviceClient));
            dispatcher.RegisterCommand(new ModCommand(serviceClient));
            dispatcher.RegisterCommand(new ExitCommand());

            var repl = new DargonREPL(dispatcher);
            repl.Run();
            return 0;
         }
      }

      private static IServiceClient TryConnectToEndpoint(int reconnectAttempts, ServiceClientFactory serviceClientFactory, ITcpEndPoint endpoint, DargonServiceConfiguration serviceConfiguration) {
         IServiceClient serviceClient = null;
         for (var i = 0; i < reconnectAttempts && serviceClient == null; i++) {
            try {
               serviceClient = serviceClientFactory.Create(endpoint);
            } catch (Exception e) {
               if (i == 0) {
                  Console.Write("Connecting to local endpoint on port " + serviceConfiguration.Port + ".");
               } else if (i > 0) {
                  Console.Write(".");
               }
            }
            if (serviceClient != null && i > 0) {
               Console.WriteLine();
            }
         }
         return serviceClient;
      }
   }
}
