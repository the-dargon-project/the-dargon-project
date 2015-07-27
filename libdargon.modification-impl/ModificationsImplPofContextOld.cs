using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Modifications {
   // POF Range 10000 - 10999
   public class ModificationsImplPofContextOld : PofContext {
      private const int kPofIdBase = 10500;
      public ModificationsImplPofContextOld() {
         RegisterPortableObjectType(kPofIdBase + 0, typeof(ModificationOld));
         RegisterPortableObjectType(kPofIdBase + 1, typeof(ModificationMetadata));
         RegisterPortableObjectType(kPofIdBase + 2, typeof(BuildConfiguration));
      }
   }
}
