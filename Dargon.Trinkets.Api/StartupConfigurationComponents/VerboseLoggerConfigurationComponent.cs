namespace Dargon.Trinkets.StartupConfigurationComponents
{
   public class VerboseLoggerConfigurationComponent : IConfigurationComponent
   {
      private const string kLogFilterPropertyName = "log_filter";
      private const string kVerboseValueName = "verbose";

      public void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder) { builder.SetProperty(kLogFilterPropertyName, kVerboseValueName); }
   }
}
