using Dargon.PortableObjects;

namespace Dargon.Trinkets.Components {
   public class FilesystemComponent : TrinketComponent {
      private const string kEnableFilesystemHooksFlag = "--enable-filesystem-hooks";
      private const string kEnableFilesystemOverridesFlag = "--enable-filesystem-mods";

      public FilesystemComponent() { }

      public FilesystemComponent(bool overridingEnabled) {
         this.OverridingEnabled = overridingEnabled;
      }

      public bool OverridingEnabled { get; private set; }

      public void HandleBootstrap(ManageableBootstrapConfiguration configuration) {
         configuration.SetFlag(kEnableFilesystemHooksFlag);
         if (OverridingEnabled) {
            configuration.SetFlag(kEnableFilesystemOverridesFlag);
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteBoolean(0, OverridingEnabled);
      }

      public void Deserialize(IPofReader reader) {
         OverridingEnabled = reader.ReadBoolean(0);
      }
   }
}
