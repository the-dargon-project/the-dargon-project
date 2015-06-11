namespace Dargon.Trinkets.StartupConfigurationComponents
{
   public interface IConfigurationComponent
   {
      void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder);
   }
}
