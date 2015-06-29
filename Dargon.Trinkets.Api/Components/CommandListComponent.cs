using Dargon.PortableObjects;
using Dargon.Trinkets.Commands;

namespace Dargon.Trinkets.Components {
   public class CommandListComponent : TrinketComponent {
      private const string kEnableCommandListFlag = "--enable-dim-command-list";
      private CommandList commandList;

      public CommandListComponent() {

      }

      public CommandListComponent(CommandList commandList) {
         this.commandList = commandList;
      }

      public CommandList CommandList => commandList;

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetFlag(kEnableCommandListFlag);
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteObject(0, commandList);
      }
      public void Deserialize(IPofReader reader) {
         commandList = reader.ReadObject<CommandList>(0);
      }
   }
}
