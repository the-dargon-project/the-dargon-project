using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Management;

namespace Dargon {
   public class SystemStateMob {
      private readonly SystemState systemState;

      public SystemStateMob(SystemState systemState) {
         this.systemState = systemState;
      }

      [ManagedOperation]
      public string Get(string key) {
         return systemState.Get(key, null);
      }

      [ManagedOperation]
      public bool Set(string key, string value) {
         systemState.Set(key, value);
         return true;
      }
   }
}
