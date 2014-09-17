namespace Dargon.InjectedModule
{
   public class BootstrapConfiguration
   {
      private readonly string args;

      public BootstrapConfiguration(string args) { this.args = args; }

      public string Args { get { return args; } }
   }
}