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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Threading;
using Dargon.Daemon;
using Dargon.Manager.Controllers;
using Dargon.Manager.Models;
using Dargon.Manager.ViewModels;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using ItzWarty;

namespace Dargon.Manager {
   public class DargonManagerApplicationEgg : INestApplicationEgg {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IClientConfiguration clientConfiguration;
      private readonly DaemonService daemonService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly List<object> keepalive = new List<object>();

      public DargonManagerApplicationEgg() {
         ICollectionFactory collectionFactory = new CollectionFactory();
         ProxyGenerator proxyGenerator = new ProxyGenerator();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         fileSystemProxy = new FileSystemProxy(streamFactory);
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IPofContext pofContext = new ClientPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         clientConfiguration = new ClientConfiguration();
         var clusteringConfiguration = new ClientClusteringConfiguration();
         var serviceClientFactory = new ServiceClientFactory(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         var serviceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);

         daemonService = serviceClient.GetService<DaemonService>();
         modificationRepositoryService = serviceClient.GetService<ModificationRepositoryService>();
      }

      public NestResult Start(IEggParameters parameters) {
         keepalive.Add(parameters);

         var dispatcherReadySignal = new ManualResetEvent(false);
         var userInterfaceThread = new Thread(() => UserInterfaceThreadStart(dispatcherReadySignal));
         userInterfaceThread.SetApartmentState(ApartmentState.STA);
         userInterfaceThread.Start();
         dispatcherReadySignal.WaitOne();

         return NestResult.Success;
      }

      public void UserInterfaceThreadStart(ManualResetEvent dispatcherReadySignal) {
         Application application = Application.Current;
         if (application == null) {
            // This initializes the Application.Current singleton too
            application = new Application();
         }

         Dispatcher dispatcher = application.Dispatcher;
         dispatcherReadySignal.Set();

         var statusController = new StatusController(new StatusModelImpl());
         Modification2Factory modificationFactory = new Modification2Factory(fileSystemProxy, new ModificationMetadataSerializer(fileSystemProxy));
         var localRepositoryModificationList = new LocalRepositoryModificationList(fileSystemProxy, clientConfiguration, modificationFactory);
         localRepositoryModificationList.Initialize();
         ModificationListingViewModel modificationListingViewModel = new ModificationListingViewModel(new ImportValidityModelImpl(), localRepositoryModificationList);
         ModificationImportController modificationImportController = new ModificationImportController(statusController, new ImportValidityModelImpl());
         var rootController = new RootController(daemonService, statusController, modificationListingViewModel, modificationImportController);

         Console.WriteLine("BeginInvoking: ");
         dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
            Console.WriteLine("BeginInvoked!");
            var window = new MainWindow(rootController);
            ElementHost.EnableModelessKeyboardInterop(window);
            window.Show();
         }));

         application.Run();
      }

      public NestResult Shutdown() {
         Application.Current.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
         Application.Current.Dispatcher.Thread.Join();
         return NestResult.Success;
      }
   }
}
