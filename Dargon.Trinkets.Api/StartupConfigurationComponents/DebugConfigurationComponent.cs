namespace Dargon.Trinkets.StartupConfigurationComponents
{
   public class DebugConfigurationComponent : IConfigurationComponent
   {
      private const string kDebugFlag = "--debug";

      public void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder) { builder.SetFlag(kDebugFlag); }
   }
}
