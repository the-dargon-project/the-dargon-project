using ItzWarty.Collections;
using SCG = System.Collections.Generic;

namespace Dargon.Trinkets {
   public class BootstrapConfigurationImpl : ManageableBootstrapConfiguration {
      private readonly ISet<string> flags;
      private readonly SCG.IDictionary<string, string> properties;

      public BootstrapConfigurationImpl()
      : this(new HashSet<string>(), new SCG.Dictionary<string, string>()) {}

      internal BootstrapConfigurationImpl(
         ISet<string> flags, 
         SCG.IDictionary<string, string> properties
      ) {
         this.flags = flags;
         this.properties = properties;
      }

      public IReadOnlySet<string> Flags => flags;
      public SCG.IDictionary<string, string> Properties => properties;

      public void SetFlag(string flag) {
         flags.Add(flag);
      }

      public void SetProperty(string key, string value) {
         properties.Add(key, value);
      }
   }
}