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
using Castle.Components.DictionaryAdapter.Xml;
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
using Dargon.LeagueOfLegends;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using Dargon.Modifications.ThumbnailGenerator;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.PortableObjects.Streams;
using Dargon.Services;
using Dargon.Trinkets.Commands;
using ItzWarty.Collections;
using ItzWarty.Networking;
using ItzWarty.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Dargon.Client {
   public class DargonClientEgg : INestApplicationEgg {
      private const string kRepositoryDirectoryName = "repositories";

      private readonly IFileSystemProxy fileSystemProxy;
      private readonly DriveNodeFactory driveNodeFactory;
      private readonly RiotSolutionLoader riotSolutionLoader;
      private readonly IPofContext pofContext;
      private readonly IPofSerializer pofSerializer;
      private readonly IClientConfiguration clientConfiguration;
      private readonly ModificationLoader modificationLoader;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ExeggutorService exeggutorService;
      private readonly LeagueBuildUtilities leagueBuildUtilities;
      private readonly List<object> keepalive = new List<object>();

      public DargonClientEgg() {
         InitializeLogging();

         IStreamFactory streamFactory = new StreamFactory();
         ICollectionFactory collectionFactory = new CollectionFactory();
         IThreadingProxy threadingProxy = new ThreadingProxy(new ThreadingFactory(), new SynchronizationFactory());
         IDnsProxy dnsProxy = new DnsProxy();
         INetworkingProxy networkingProxy = new NetworkingProxy(new SocketFactory(new TcpEndPointFactory(dnsProxy), new NetworkingInternalFactory(threadingProxy, streamFactory)), new TcpEndPointFactory(dnsProxy));
         fileSystemProxy = new FileSystemProxy(streamFactory);
         driveNodeFactory = new DriveNodeFactory(streamFactory);
         riotSolutionLoader = new RiotSolutionLoader();

         pofContext = new PofContext().With(x => {
            x.MergeContext(new ClientPofContext());
            x.MergeContext(new ThumbnailGeneratorApiPofContext());
         });
         pofSerializer = new PofSerializer(pofContext);
         PofStreamsFactory pofStreamsFactory = new PofStreamsFactoryImpl(threadingProxy, streamFactory, pofSerializer);

         IClusteringConfiguration clusteringConfiguration = new ClientClusteringConfiguration();
         IServiceClientFactory serviceClientFactory = new ServiceClientFactory(new ProxyGenerator(), streamFactory, collectionFactory, threadingProxy, networkingProxy, pofSerializer, pofStreamsFactory);
         IServiceClient localServiceClient = serviceClientFactory.CreateOrJoin(clusteringConfiguration);
         keepalive.Add(localServiceClient);

         clientConfiguration = new ClientConfiguration();
         ModificationComponentFactory modificationComponentFactory = new ModificationComponentFactory(fileSystemProxy, pofContext, new SlotSourceFactoryImpl(), pofSerializer);
         modificationLoader = new ModificationLoaderImpl(clientConfiguration.RepositoriesDirectoryPath, modificationComponentFactory);

         temporaryFileService = localServiceClient.GetService<TemporaryFileService>();
         exeggutorService = localServiceClient.GetService<ExeggutorService>();

         SystemState systemState = new ClientSystemStateImpl(fileSystemProxy, clientConfiguration.ConfigurationDirectoryPath);
         LeagueConfiguration leagueConfiguration = new LeagueConfiguration();
         CommandFactory commandFactory = new CommandFactoryImpl();
         LeagueBuildUtilitiesConfiguration leagueBuildUtilitiesConfiguration = new LeagueBuildUtilitiesConfiguration(systemState);
         leagueBuildUtilities = new LeagueBuildUtilities(systemState, leagueConfiguration, fileSystemProxy, riotSolutionLoader, temporaryFileService, commandFactory, leagueBuildUtilitiesConfiguration);
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
         ObservableCollection<ModificationViewModel> modificationViewModels = new ObservableCollection<ModificationViewModel>();
         var rootViewModelCommandFactory = new ModificationImportController(pofSerializer, repositoriesDirectory, temporaryFileService, exeggutorService, modificationComponentFactory, fileSystemProxy, riotSolutionLoader, modificationImportViewModelFactory, modificationViewModels, modificationLoader, leagueBuildUtilities);
         var modificationListingSynchronizer = new ModificationListingSynchronizer(pofSerializer, fileSystemProxy, clientConfiguration, temporaryFileService, exeggutorService, modificationLoader, modificationViewModels, leagueBuildUtilities);
         modificationListingSynchronizer.Initialize();
         var rootViewModel = new RootViewModel(rootViewModelCommandFactory, window, modificationViewModels);
         window.DataContext = rootViewModel;
         application.Run(window);
      }

      public NestResult Shutdown() {
         return NestResult.Success;
      }

      private void InitializeLogging() {
         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget() {
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };
         Target consoleTarget = new ColoredConsoleTarget() {
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };

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