namespace Dargon.Trinkets.StartupConfigurationComponents
{
   public class FilesystemConfigurationComponent : IConfigurationComponent
   {
      private const string kEnableFilesystemHooksFlag = "--enable-filesystem-hooks";
      private const string kEnableFilesystemOverridesFlag = "--enable-filesystem-mods";

      private readonly bool overridingEnabled;
      
      public FilesystemConfigurationComponent(bool overridingEnabled) { this.overridingEnabled = overridingEnabled; }

      public bool OverridingEnabled { get { return overridingEnabled; } }

      public void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder) 
      {
         builder.SetFlag(kEnableFilesystemHooksFlag);
         if (overridingEnabled) {
            builder.SetFlag(kEnableFilesystemOverridesFlag);
         }
      }
   }
}
