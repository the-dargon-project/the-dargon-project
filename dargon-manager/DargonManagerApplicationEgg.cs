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
      public NestResult Start(IEggParameters parameters) {
         ICollectionFactory collectionFactory = new CollectionFactory();
         ProxyGenerator proxyGenerator = new ProxyGenerator();
         IThreadingFactory threadingFactory = new ThreadingFactory();
         ISynchronizationFactory synchronizationFactory = new SynchronizationFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(threadingFactory, synchronizationFactory);
         IDnsProxy dnsProxy = new DnsProxy();
         ITcpEndPointFactory tcpEndPointFactory = new TcpEndPointFactory(dnsProxy);
         IStreamFactory streamFactory = new StreamFactory();
         IFileSystemProxy fileSystemProxy = new FileSystemProxy(streamFactory);
         INetworkingInternalFactory networkingInternalFactory = new NetworkingInternalFactory(threadingProxy, streamFactory);
         ISocketFactory socketFactory = new SocketFactory(tcpEndPointFactory, networkingInternalFactory);
         INetworkingProxy networkingProxy = new NetworkingProxy(socketFactory, tcpEndPointFactory);
         IPofContext pofContext = new ClientPofContext();
         IPofSerializer pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         IClientConfiguration clientConfiguration = new ClientConfiguration();

         var clusteringConfiguration = new ClientClusteringConfiguration();
         var serviceClientFactory = new ServiceClientFactory(proxyGenerator, streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         var serviceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
//         Thread.Sleep(2000);
         Console.WriteLine("!A");
         var repositoryService = serviceClient.GetService<ModificationRepositoryService>();
         Console.WriteLine("!B");
         Console.WriteLine(repositoryService.EnumerateModifications().ToArray());
         //Console.WriteLine(daemonService.Configuration.AppDataDirectoryPath);
         Console.WriteLine("!C");

         var daemonService = serviceClient.GetService<DaemonService>();
         var modificationRepositoryService = serviceClient.GetService<ModificationRepositoryService>();
         var statusController = new StatusController(new StatusModelImpl());
         IModificationLoader modificationLoader = new ModificationLoader(new ModificationMetadataSerializer(fileSystemProxy), new BuildConfigurationLoader());
         ModificationListingViewModel modificationListingViewModel = new ModificationListingViewModel(new ImportValidityModelImpl(), new LocalRepositoryModificationList(fileSystemProxy, clientConfiguration, modificationLoader));
         ModificationImportController modificationImportController = new ModificationImportController(statusController, new ImportValidityModelImpl());
         var rootController = new RootController(daemonService, statusController, modificationListingViewModel, modificationImportController);
         new Thread(
            () => {
               if (Application.Current == null)
                  new Application();

               Console.WriteLine("BeginInvoking: ");
               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                  Console.WriteLine("BeginInvoked!");
                  var window = new MainWindow(rootController);
                  ElementHost.EnableModelessKeyboardInterop(window);
                  window.Show();
                  //                  new DargonManagerApplication(serviceClient).Run();
               }));

               Dispatcher.Run();
               GC.KeepAlive(parameters);
            }).With(t => {
               t.SetApartmentState(ApartmentState.STA);
            }).Start();
         return NestResult.Success;
      }

      public NestResult Shutdown() {
         throw new NotImplementedException();
      }
   }
}
