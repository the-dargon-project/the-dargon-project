using System;
using System.IO;
using ItzWarty.Collections;

namespace Dargon.Modifications {
   public class ModificationImpl : Modification {
      private readonly string repositoryName;
      private readonly string repositoryPath;
      private readonly ModificationComponentFactory modificationComponentFactory;
      private readonly IConcurrentDictionary<Type, Component> componentsByType = new ConcurrentDictionary<Type, Component>();

      public ModificationImpl(string repositoryName, string repositoryPath, ModificationComponentFactory modificationComponentFactory) {
         this.repositoryName = repositoryName;
         this.repositoryPath = repositoryPath;
         this.modificationComponentFactory = modificationComponentFactory;
      }

      public string RepositoryName => repositoryName;
      public string RepositoryPath => repositoryPath;
      public string MetadataPath => Path.Combine(repositoryPath, "metadata");

      public TComponent GetComponent<TComponent>() where TComponent : Component {
         return (TComponent)componentsByType.GetOrAdd(
            typeof(TComponent),
            (add) => modificationComponentFactory.Create<TComponent>(this, MetadataPath)
         );
      }
   }
}
