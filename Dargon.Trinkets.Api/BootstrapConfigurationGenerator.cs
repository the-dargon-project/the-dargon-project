using System.Collections.Generic;
using Dargon.Trinkets.Components;
using ItzWarty.Collections;

namespace Dargon.Trinkets {
   public interface BootstrapConfigurationGenerator {
      ReadableBootstrapConfiguration Build(IReadOnlyCollection<TrinketComponent> components);
   }
}
