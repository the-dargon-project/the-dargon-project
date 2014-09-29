using System.Collections.Generic;

namespace Dargon.InjectedModule
{
   public class BootstrapConfiguration
   {
      private readonly ISet<string> flags;
      private readonly IReadOnlyDictionary<string, string> properties;
      
      public BootstrapConfiguration(ISet<string> flags, IReadOnlyDictionary<string, string> properties)
      {
         this.flags = flags;
         this.properties = properties;
      }

      public ISet<string> Flags { get { return flags; } }
      public IReadOnlyDictionary<string, string> Properties { get { return properties; } }
   }
}