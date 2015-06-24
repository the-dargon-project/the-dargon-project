using Dargon.PortableObjects;

namespace Dargon.Trinkets.Components {
   public interface TrinketComponent : IPortableObject {
      void HandleBootstrap(ManageableBootstrapConfiguration configuration);
   }
}
