using Dargon.Manager.Models;
using Dargon.ModificationRepositories;
using ItzWarty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dargon.Modifications;
using ItzWarty.IO;

namespace Dargon.Manager.Controllers {
   public sealed class LocalRepositoryModificationList : SynchronizedModificationList {
      private const string kRepositoryDirectoryName = "repositories";
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IClientConfiguration clientConfiguration;
      private readonly IModificationLoader modificationLoader;
      private FileSystemWatcher watcher;

      public LocalRepositoryModificationList(IFileSystemProxy fileSystemProxy, IClientConfiguration clientConfiguration, IModificationLoader modificationLoader) {
         this.fileSystemProxy = fileSystemProxy;
         this.clientConfiguration = clientConfiguration;
         this.modificationLoader = modificationLoader;
      }

      public string RepositoriesDirectoryPath => Path.Combine(clientConfiguration.UserDataDirectoryPath, kRepositoryDirectoryName);

      public void Initialize() {
         Fetch();

         watcher = new FileSystemWatcher(RepositoriesDirectoryPath);
         watcher.IncludeSubdirectories = false;
         watcher.EnableRaisingEvents = true;
         watcher.Changed += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Fetch));
         watcher.Created += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Fetch));
         watcher.Deleted += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Fetch));
         watcher.Renamed += (s, e) => Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Fetch));
      }

      protected override IReadOnlyList<ModificationViewModel> FetchInternal() {
         Console.WriteLine("FETCH!");
         var repositoriesDirectory = fileSystemProxy.GetDirectoryInfo(RepositoriesDirectoryPath);
         var results = new List<ModificationViewModel>();
         foreach (var repositoryDirectory in repositoriesDirectory.EnumerateDirectories()) {
            results.Add(new ModificationViewModel(modificationLoader.Load(repositoryDirectory.Name, repositoryDirectory.FullName)));
         }
         return results;
      }
   }
}