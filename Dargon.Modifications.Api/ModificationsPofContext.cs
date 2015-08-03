using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Modifications {
   public class ModificationsPofContext : PofContext {
      private const int kBasePofId = 10000;
      public ModificationsPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(InfoComponent));
         RegisterPortableObjectType(kBasePofId + 1, typeof(ThumbnailComponent));
      }
   }
}
