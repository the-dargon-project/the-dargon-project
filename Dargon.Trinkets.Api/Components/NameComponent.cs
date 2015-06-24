using Dargon.PortableObjects;

namespace Dargon.Trinkets.Components {
   public class NameComponent : TrinketComponent {
      private const string kNamePropertyName = "name";
      
      private string value;

      public NameComponent() { }

      public NameComponent(string value) {
         this.value = value;
      }

      public string Value => value;

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetProperty(kNamePropertyName, value);
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, value);
      }
      public void Deserialize(IPofReader reader) {
         value = reader.ReadString(0);
      }
   }
}
