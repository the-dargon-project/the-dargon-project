using System.Collections.Generic;
using System.Linq;
using ItzWarty;

namespace Dargon.InjectedModule
{
   public class BootstrapConfigurationBuilder
   {
      private readonly ISet<string> flags = new HashSet<string>();
      private readonly Dictionary<string, string> properties = new Dictionary<string, string>(); 

      public void SetFlag(string flag) { flags.Add(flag); }
      public void SetProperty(string key, string value) { properties.Add(key, value); }

      public BootstrapConfiguration Build() { return new BootstrapConfiguration(flags, properties); }
   }
}