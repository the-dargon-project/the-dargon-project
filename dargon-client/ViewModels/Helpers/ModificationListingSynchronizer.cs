﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dargon.Modifications;
using Dargon.Services;
using ItzWarty.IO;
using NLog;

namespace Dargon.Client.ViewModels.Helpers {
   public class ModificationListingSynchronizer {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const string kRepositoryDirectoryName = "repositories";
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IClientConfiguration clientConfiguration;
      private readonly Modification2Factory modificationFactory;
      private readonly ObservableCollection<ModificationViewModel> modificationViewModels;
      private FileSystemWatcher watcher;

      public ModificationListingSynchronizer(IClientConfiguration clientConfiguration, IFileSystemProxy fileSystemProxy, Modification2Factory modificationFactory, ObservableCollection<ModificationViewModel> modificationViewModels) {
         this.clientConfiguration = clientConfiguration;
         this.modificationViewModels = modificationViewModels;
         this.fileSystemProxy = fileSystemProxy;
         this.modificationFactory = modificationFactory;
      }

      public string RepositoriesDirectoryPath => Path.Combine(clientConfiguration.UserDataDirectoryPath, kRepositoryDirectoryName);

      public void Initialize() {
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

      private void HandleRepositoriesDirectoryModified(object sender, FileSystemEventArgs e) {
         switch (e.ChangeType) {
            case WatcherChangeTypes.Changed:
               break;
            case WatcherChangeTypes.Created:
               var viewModel = new ModificationViewModel(modificationFactory.FromLocalRepository(e.FullPath));
               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                  modificationViewModels.Add(viewModel);
               }));
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