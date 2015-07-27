using System;
using System.IO;
using ItzWarty.Collections;

namespace Dargon.Modifications {
   public class Modification {
      private readonly string repositoryName;
      private readonly string repositoryPath;
      private readonly ModificationComponentFactory modificationComponentFactory;
      private readonly IConcurrentDictionary<Type, Component> componentsByType = new ConcurrentDictionary<Type, Component>();

      public Modification(string repositoryName, string repositoryPath, ModificationComponentFactory modificationComponentFactory) {
         this.repositoryName = repositoryName;
         this.repositoryPath = repositoryPath;
         this.modificationComponentFactory = modificationComponentFactory;
      }

      public string RepositoryName => repositoryName;
      public string RepositoryPath => repositoryPath;
      public string MetadataPath => Path.Combine(repositoryPath, "metadata");

      public TComponent GetComponent<TComponent>() where TComponent : Component, new() {
         return (TComponent)componentsByType.GetOrAdd(
            typeof(TComponent),
            (add) => modificationComponentFactory.Create<TComponent>(MetadataPath)
         );
      }
   }

   public class ModificationFactory {
      private readonly ModificationComponentFactory modificationComponentFactory;

      public ModificationFactory(ModificationComponentFactory modificationComponentFactory) {
         this.modificationComponentFactory = modificationComponentFactory;
      }

      public Modification FromLocalDirectory(string path) {
         var directoryInfo = new DirectoryInfo(path);
         return new Modification(directoryInfo.Name, directoryInfo.FullName, modificationComponentFactory);
      }
   }
}