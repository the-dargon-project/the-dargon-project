using Dargon.PortableObjects;

namespace Dargon.Trinkets.Components {
   public class DebugComponent : TrinketComponent {
      private const string kDebugFlag = "--debug";

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetFlag(kDebugFlag);
      }

      public void Serialize(IPofWriter writer) { }
      public void Deserialize(IPofReader reader) { }
   }
}
