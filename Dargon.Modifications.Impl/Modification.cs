using System;
using ItzWarty.Collections;

namespace Dargon.Modifications.Impl {
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

      public TComponent GetComponent<TComponent>() where TComponent : Component, new() {
         return (TComponent)componentsByType.GetOrAdd(
            typeof(TComponent), 
            (add) => modificationComponentFactory.Create<TComponent>()
         );
      }
   }
}