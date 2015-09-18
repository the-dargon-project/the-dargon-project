using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Management;

namespace Dargon {
   public class ClientSystemStateMob {
      private readonly SystemState systemState;

      public ClientSystemStateMob(SystemState systemState) {
         this.systemState = systemState;
      }

      [ManagedOperation]
      public string Get(string key) => systemState.Get(key, null);

      [ManagedOperation]
      public void Set(string key, string value) => systemState.Set(key, value);
   }
}
