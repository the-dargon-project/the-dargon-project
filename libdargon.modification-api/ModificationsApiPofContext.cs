using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Modifications {
   public class ModificationsApiPofContext : PofContext {
      private const int kPofIdBase = 10000;
      public ModificationsApiPofContext() {
         RegisterPortableObjectType(kPofIdBase + 0, typeof(IModification));
      }
   }
}
