using Dargon.PortableObjects;

namespace Dargon.Trinkets.Components {
   public class VerboseLoggerComponent : TrinketComponent {
      private const string kLogFilterPropertyName = "log_filter";
      private const string kVerboseValueName = "verbose";

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetProperty(kLogFilterPropertyName, kVerboseValueName);
      }

      public void Serialize(IPofWriter writer) { }
      public void Deserialize(IPofReader reader) { }
   }
}
