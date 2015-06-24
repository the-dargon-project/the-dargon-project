using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Trinkets.Components {
   public class ProcessSuspensionComponent : TrinketComponent {
      private const string kEnableProcessSuspensionFlagName = "--enable-process-suspension";
      private const string kSuspendedProcessesPropertyName = "launchsuspended";

      private IReadOnlySet<string> processNames;

      public ProcessSuspensionComponent() { }

      public ProcessSuspensionComponent(IReadOnlySet<string> processNames) {
         this.processNames = processNames;
      }

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetFlag(kEnableProcessSuspensionFlagName);
         configuration.SetProperty(kSuspendedProcessesPropertyName, processNames.Join(","));
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteCollection(0, processNames);
      }
      public void Deserialize(IPofReader reader) {
         processNames = reader.ReadCollection<string, HashSet<string>>(0);
      }
   }
}
