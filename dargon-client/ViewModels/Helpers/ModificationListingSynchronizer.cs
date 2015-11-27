using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dargon.Client.Controllers;
using Dargon.Client.Controllers.Phases;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.Services;
using ItzWarty;
using ItzWarty.IO;
using NLog;

namespace Dargon.Client.ViewModels.Helpers {
   public class ModificationListingSynchronizer {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const string kRepositoryDirectoryName = "repositories";
      private readonly HashSet<string> displayedModificationNames = new HashSet<string>(); 
      private readonly IPofSerializer pofSerializer;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IClientConfiguration clientConfiguration;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ExeggutorService exeggutorService;
      private readonly ModificationLoader modificationLoader;
      private readonly ObservableCollection<ModificationViewModel> modificationViewModels;
      private readonly LeagueBuildUtilities leagueBuildUtilities;
      private FileSystemWatcher watcher;

      public ModificationListingSynchronizer(IPofSerializer pofSerializer, IFileSystemProxy fileSystemProxy, IClientConfiguration clientConfiguration, TemporaryFileService temporaryFileService, ExeggutorService exeggutorService, ModificationLoader modificationLoader, ObservableCollection<ModificationViewModel> modificationViewModels, LeagueBuildUtilities leagueBuildUtilities) {
         this.pofSerializer = pofSerializer;
         this.fileSystemProxy = fileSystemProxy;
         this.clientConfiguration = clientConfiguration;
         this.temporaryFileService = temporaryFileService;
         this.exeggutorService = exeggutorService;
         this.modificationLoader = modificationLoader;
         this.modificationViewModels = modificationViewModels;
         this.leagueBuildUtilities = leagueBuildUtilities;
      }

      public string RepositoriesDirectoryPath => Path.Combine(clientConfiguration.UserDataDirectoryPath, kRepositoryDirectoryName);

      public void Initialize() {
         modificationViewModels.CollectionChanged += HandleViewModelCollectionChanged;

         fileSystemProxy.PrepareDirectory(RepositoriesDirectoryPath);

         watcher = new FileSystemWatcher(RepositoriesDirectoryPath);
         watcher.IncludeSubdirectories = false;
         watcher.EnableRaisingEvents = true;
         watcher.Changed += HandleRepositoriesDirectoryModified;
         watcher.Created += HandleRepositoriesDirectoryModified;
         watcher.Deleted += HandleRepositoriesDirectoryModified;
         watcher.Renamed += HandleRepositoriesDirectoryModified;

         foreach (var directory in fileSystemProxy.GetDirectoryInfo(RepositoriesDirectoryPath).EnumerateDirectories()) {
            HandleRepositoriesDirectoryModified(null, new FileSystemEventArgs(WatcherChangeTypes.Created, directory.Parent.FullName, directory.Name));
         }
      }

      private void HandleViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
         e.OldItems?.Cast<ModificationViewModel>().ForEach(vm => displayedModificationNames.Remove(vm.RepositoryName));
         e.NewItems?.Cast<ModificationViewModel>().ForEach(vm => displayedModificationNames.Add(vm.RepositoryName));
      }

      private void HandleRepositoriesDirectoryModified(object sender, FileSystemEventArgs e) {
         switch (e.ChangeType) {
            case WatcherChangeTypes.Changed:
               break;
            case WatcherChangeTypes.Created:
               var fileInfo = fileSystemProxy.GetFileInfo(e.FullPath);
               if (fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
                  if (fileInfo.Name.StartsWith(".")) {
                     logger.Info($"Ignoring modification candidate {fileInfo.Name} as name has leading period.");
                  } else if (displayedModificationNames.Contains(fileInfo.Name)) {
                     logger.Info($"Ignoring modification candidate {fileInfo.Name} as view model already exists.");
                  } else {
                     var modification = modificationLoader.FromPath(e.FullPath);
                     var viewModel = new ModificationViewModel();
                     var controller = new ModificationController(modification, viewModel);
                     viewModel.SetController(controller);
                     viewModel.SetModification(modification);
                     var modificationPhaseManager = new ModificationPhaseManager();
                     var modificationPhaseFactory = new ModificationPhaseFactory(pofSerializer, fileSystemProxy, temporaryFileService, exeggutorService, modificationPhaseManager, modificationLoader, viewModel, leagueBuildUtilities, modification);
                     modificationPhaseManager.Transition(modificationPhaseFactory.Idle());
                     controller.SetModificationPhaseManager(modificationPhaseManager);
                     controller.Initialize();

                     Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                        modificationViewModels.Add(viewModel);
                     }));
                  }
               }
               break;
            case WatcherChangeTypes.Deleted:
               var removedViewModel = modificationViewModels.FirstOrDefault(x => x.RepositoryName.Equals(e.Name, StringComparison.OrdinalIgnoreCase));
               if (removedViewModel == null) {
                  logger.Error("Failed to find viewmodel match for removed directory " + e.FullPath);
               } else {
                  Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                     modificationViewModels.Remove(removedViewModel);
                  }));
               }
               break;
            case WatcherChangeTypes.Renamed:
               var renamedArgs = (RenamedEventArgs)e;
               var parentDirectory = new FileInfo(renamedArgs.FullPath).Directory;
               HandleRepositoriesDirectoryModified(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, parentDirectory.FullName, renamedArgs.OldName));
               HandleRepositoriesDirectoryModified(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, parentDirectory.FullName, renamedArgs.Name));
               break;
         }
      }
   }
}
