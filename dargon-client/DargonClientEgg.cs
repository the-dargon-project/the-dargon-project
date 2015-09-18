using Dargon.Client.Controllers;
using Dargon.Client.ViewModels;
using Dargon.Client.ViewModels.Helpers;
using Dargon.Client.Views;
using Dargon.IO.Drive;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using Dargon.Nest.Egg;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.RADS;
using Dargon.Ryu;
using Dargon.Services;
using ItzWarty.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using Dargon.Management.Server;
using ItzWarty;
using ItzWarty.Networking;

namespace Dargon.Client {
   public class DargonClientEgg : INestApplicationEgg {
      private int kClientManagementPort = 21002;
      private const string kRepositoryDirectoryName = "repositories";

      private readonly RyuContainer ryu; 
      private IEggHost host;

      public DargonClientEgg() {
         InitializeLogging();
         ryu = new RyuFactory().Create();
         ((RyuContainerImpl)ryu).SetLoggerEnabled(true);
      }

      public NestResult Start(IEggParameters parameters) {
         this.host = parameters?.Host;

         ryu.Touch<ItzWartyCommonsRyuPackage>();
         ryu.Touch<ItzWartyProxiesRyuPackage>();

         // Dargon.management
         var managementServerEndpoint = ryu.Get<INetworkingProxy>().CreateAnyEndPoint(kClientManagementPort);
         ryu.Set<IManagementServerConfiguration>(new ManagementServerConfiguration(managementServerEndpoint));

         ((RyuContainerImpl)ryu).Setup(true);

         var userInterfaceThread = new Thread(UserInterfaceThreadStart);
         userInterfaceThread.SetApartmentState(ApartmentState.STA);
         userInterfaceThread.Start();
         return NestResult.Success;
      }

      private void UserInterfaceThreadStart() {
         var clientConfiguration = ryu.Get<IClientConfiguration>();
         var fileSystemProxy = ryu.Get<IFileSystemProxy>();
         var driveNodeFactory = new DriveNodeFactory(ryu.Get<IStreamFactory>());
         var pofContext = ryu.Get<IPofContext>();
         var pofSerializer = ryu.Get<IPofSerializer>();
         var temporaryFileService = ryu.Get<ServiceClient>().GetService<TemporaryFileService>();
         var exeggutorService = ryu.Get<ServiceClient>().GetService<ExeggutorService>();
         var riotSolutionLoader = ryu.Get<RiotSolutionLoader>();
         var modificationLoader = ryu.Get<ModificationLoader>();
         var leagueBuildUtilities = ryu.Get<LeagueBuildUtilities>();

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
         Shutdown();
      }

      public NestResult Shutdown() {
         var application = Application.Current;
         application.Dispatcher.Invoke(() => { application.Shutdown(); });
         host?.Shutdown();
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