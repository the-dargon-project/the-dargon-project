using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Modifications {
   // POF Range 1001 - 2000
   public class ModificationPofContext : PofContext {
      private const int kPofIdBase = 1001;
      public ModificationPofContext() {
         RegisterPortableObjectType(kPofIdBase + 0, typeof(Modification));
         RegisterPortableObjectType(kPofIdBase + 1, typeof(ModificationMetadata));
         RegisterPortableObjectType(kPofIdBase + 2, typeof(BuildConfiguration));
      }
   }
}
