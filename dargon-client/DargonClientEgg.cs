using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dargon.Client.Views;
using Dargon.Nest.Egg;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Castle.DynamicProxy;
using Dargon.Client.Controllers;
using Dargon.Client.ViewModels;
using Dargon.Client.ViewModels.Helpers;
using Dargon.FileSystems;
using Dargon.IO;
using Dargon.IO.Drive;
using Dargon.RADS;
using ItzWarty;
using ItzWarty.IO;
using Dargon.IO.Resolution;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;

namespace Dargon.Client {
   public class DargonClientEgg : INestApplicationEgg {
      private const string kRepositoryDirectoryName = "repositories";

      private readonly IFileSystemProxy fileSystemProxy;
      private readonly DriveNodeFactory driveNodeFactory;
      private readonly RiotSolutionLoader riotSolutionLoader;
      private readonly IPofContext pofContext;
      private readonly IPofSerializer pofSerializer;
      private readonly IClientConfiguration clientConfiguration;
      private readonly ModificationFactory modificationFactory;
      private readonly TemporaryFileService temporaryFileService;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly List<object> keepalive = new List<object>();

      public DargonClientEgg() {
         IStreamFactory streamFactory = new StreamFactory();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(new ThreadingFactory(), new SynchronizationFactory());
         IDnsProxy dnsProxy = new DnsProxy();
         INetworkingProxy networkingProxy = new NetworkingProxy(new SocketFactory(new TcpEndPointFactory(dnsProxy), new NetworkingInternalFactory(threadingProxy, streamFactory)), new TcpEndPointFactory(dnsProxy));
         fileSystemProxy = new FileSystemProxy(streamFactory);
         driveNodeFactory = new DriveNodeFactory(streamFactory);
         riotSolutionLoader = new RiotSolutionLoader();

         pofContext = new ClientPofContext();
         pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         IClusteringConfiguration clusteringConfiguration = new ClientClusteringConfiguration();
         IServiceClientFactory serviceClientFactory = new ServiceClientFactory(new ProxyGenerator(), streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         IServiceClient localServiceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         keepalive.Add(localServiceClient);

         clientConfiguration = new ClientConfiguration();
         ModificationComponentFactory modificationComponentFactory = new ModificationComponentFactory(fileSystemProxy, pofContext, new SlotSourceFactoryImpl(), pofSerializer);
         modificationFactory = new ModificationFactory(modificationComponentFactory);
         temporaryFileService = localServiceClient.GetService<TemporaryFileService>();
         leagueModificationRepositoryService = localServiceClient.GetService<LeagueModificationRepositoryService>();
      }

      public NestResult Start(IEggParameters parameters) {
         var userInterfaceThread = new Thread(UserInterfaceThreadStart);
         userInterfaceThread.SetApartmentState(ApartmentState.STA);
         userInterfaceThread.Start();
         return NestResult.Success;
      }

      private void UserInterfaceThreadStart() {
         var application = Application.Current ?? new Application();
         var dispatcher = application.Dispatcher;
         var window = new MainWindow();

         var repositoriesDirectory = Path.Combine(clientConfiguration.UserDataDirectoryPath, kRepositoryDirectoryName);

         var modificationImportViewModelFactory = new ModificationImportViewModelFactory(fileSystemProxy, driveNodeFactory);
         ModificationComponentFactory modificationComponentFactory = new ModificationComponentFactory(fileSystemProxy, pofContext, new SlotSourceFactoryImpl(), pofSerializer);
         var rootViewModelCommandFactory = new ModificationImportController(repositoriesDirectory, temporaryFileService, modificationComponentFactory, fileSystemProxy, leagueModificationRepositoryService, riotSolutionLoader, modificationImportViewModelFactory);
         ObservableCollection<ModificationViewModel> modificationViewModels = new ObservableCollection<ModificationViewModel>();
         var modificationListingSynchronizer = new ModificationListingSynchronizer(clientConfiguration, fileSystemProxy, modificationFactory, modificationViewModels);
         modificationListingSynchronizer.Initialize();
         var rootViewModel = new RootViewModel(rootViewModelCommandFactory, window, modificationViewModels);
         window.DataContext = rootViewModel;
         application.Run(window);
      }

      public NestResult Shutdown() {
         return NestResult.Success;
      }
   }
}
/*
         modifications.Add(new ModificationViewModel {
            Name = "Something Ezreal",
            Author = "Herp",
            Status = ModificationStatus.Disabled,
            Type = ModificationType.Champion
         });
         modifications.Add(new ModificationViewModel {
            Name = "SR But Better",
            Author = "Lerp",
            Status = ModificationStatus.Enabled,
            Type = ModificationType.Map
         });
         modifications.Add(new ModificationViewModel {
            Name = "Warty the Ward",
            Author = "ijofgsdojmn",
            Status = ModificationStatus.Enabled,
            Type = ModificationType.Ward
         });
         modifications.Add(new ModificationViewModel {
            Name = "ItBlinksAlot UI",
            Author = "23erp",
            Status = ModificationStatus.UpdateAvailable,
            Type = ModificationType.UI
         });
         modifications.Add(new ModificationViewModel {
            Name = "Something Else",
            Author = "Perp",
            Status = ModificationStatus.Broken,
            Type = ModificationType.Other
         });

   */