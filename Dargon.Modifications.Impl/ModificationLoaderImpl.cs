using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItzWarty;

namespace Dargon.Modifications {
   public class ModificationLoaderImpl : ModificationLoader {
      private readonly string repositoriesDirectory;
      private readonly ModificationComponentFactory modificationComponentFactory;

      public ModificationLoaderImpl(string repositoriesDirectory, ModificationComponentFactory modificationComponentFactory) {
         this.repositoriesDirectory = repositoriesDirectory;
         this.modificationComponentFactory = modificationComponentFactory;
      }

      public IReadOnlyList<Modification> EnumerateModifications() {
         var repositoriesDirectoryInfo = new DirectoryInfo(repositoriesDirectory);
         if (!repositoriesDirectoryInfo.Exists) {
            repositoriesDirectoryInfo.Create();
         }

         // prevents us from loading .cache as a mod
         var repositoryDirectories = repositoriesDirectoryInfo.EnumerateDirectories().Where(di => di.Name[0] != '.');
         return repositoryDirectories.Select(di => FromPath(di.FullName)).ToArray();
      }

      public Modification FromPath(string path) {
         var directoryInfo = new DirectoryInfo(path);
         return new ModificationImpl(directoryInfo.Name, directoryInfo.FullName, modificationComponentFactory);
      }
   }
}