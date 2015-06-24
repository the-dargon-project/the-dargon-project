using Dargon.Trinkets.Components;
using ItzWarty.Collections;
using SCG = System.Collections.Generic;

namespace Dargon.Trinkets {
   public class BootstrapConfigurationGeneratorImpl : BootstrapConfigurationGenerator {
      public ReadableBootstrapConfiguration Build(SCG.IReadOnlyCollection<TrinketComponent> components) {
         var bootstrapConfiguration = new BootstrapConfigurationImpl();
         foreach (var component in components) {
            component.HandleBootstrap(bootstrapConfiguration);
         }
         return bootstrapConfiguration;
      }
   }
}