using Dargon.PortableObjects;

namespace Dargon.Trinkets.Components {
   public class CommandListComponent : TrinketComponent {
      private const string kEnableCommandListFlag = "--enable-dim-command-list";

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetFlag(kEnableCommandListFlag);
      }

      public void Serialize(IPofWriter writer) { }
      public void Deserialize(IPofReader reader) { }
   }
}
