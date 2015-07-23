using Dargon.Manager.Models;
using Dargon.ModificationRepositories;
using ItzWarty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dargon.Modifications;
using ItzWarty.IO;

namespace Dargon.Manager.Controllers {
   public sealed class LocalRepositoryModificationList : SynchronizedModificationList {
      private const string kRepositoryDirectoryName = "repositories";
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IClientConfiguration clientConfiguration;
      private readonly IModificationLoader modificationLoader;

      public LocalRepositoryModificationList(IFileSystemProxy fileSystemProxy, IClientConfiguration clientConfiguration, IModificationLoader modificationLoader) {
         this.fileSystemProxy = fileSystemProxy;
         this.clientConfiguration = clientConfiguration;
         this.modificationLoader = modificationLoader;

         Fetch();
      }

      protected override IReadOnlyList<ModificationViewModel> FetchInternal() {
         var repositoriesDirectoryPath = Path.Combine(clientConfiguration.UserDataDirectoryPath, kRepositoryDirectoryName);
         var repositoriesDirectory = fileSystemProxy.GetDirectoryInfo(repositoriesDirectoryPath);
         var results = new List<ModificationViewModel>();
         foreach (var repositoryDirectory in repositoriesDirectory.EnumerateDirectories()) {
            results.Add(new ModificationViewModel(modificationLoader.Load(repositoryDirectory.Name, repositoryDirectory.FullName)));
         }
         return results;
      }
   }
}