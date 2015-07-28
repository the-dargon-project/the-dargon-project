using System.Collections.Generic;
using System.IO;
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
         var repositoryDirectories = Directory.GetDirectories(repositoriesDirectory);
         return Util.Generate(repositoryDirectories.Length, i => FromPath(repositoryDirectories[i]));
      }

      public Modification FromPath(string path) {
         var directoryInfo = new DirectoryInfo(path);
         return new ModificationImpl(directoryInfo.Name, directoryInfo.FullName, modificationComponentFactory);
      }
   }
}