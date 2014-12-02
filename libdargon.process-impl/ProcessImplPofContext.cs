using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Processes.Injection;

namespace Dargon.Processes {
   public class ProcessImplPofContext : PofContext {
      public const int kPofIdOffset = 2001;

      public ProcessImplPofContext() {
         RegisterPortableObjectType(kPofIdOffset + 0, typeof(ProcessInjectionConfiguration));
      }
   }
}
