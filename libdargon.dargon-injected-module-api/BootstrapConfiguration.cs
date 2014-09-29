using System.Collections.Generic;

namespace Dargon.InjectedModule
{
   public class BootstrapConfiguration
   {
      private readonly ISet<string> flags;
      private readonly Dictionary<string, string> properties;
      
      public BootstrapConfiguration(ISet<string> flags, Dictionary<string, string> properties)
      {
         this.flags = flags;
         this.properties = properties;
      }

      public ISet<string> Flags { get { return flags; } }
      public Dictionary<string, string> Properties { get { return properties; } }
   }
}