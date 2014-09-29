using System.Collections.Generic;
using System.Linq;
using ItzWarty;

namespace Dargon.InjectedModule
{
   public class BootstrapConfigurationBuilder
   {
      private const string PROPERTY_KEY_VALUE_DELIMITER = "=";

      private ISet<string> flags = new HashSet<string>();
      private Dictionary<string, string> properties = new Dictionary<string, string>(); 

      public void SetArgumentFlag(string flag) { flags.Add(flag); }
      public void SetArgumentProperty(string key, string value) { properties.Add(key, value); }

      public BootstrapConfiguration Build()
      {
         return new BootstrapConfiguration(flags, properties);
      }
   }
}