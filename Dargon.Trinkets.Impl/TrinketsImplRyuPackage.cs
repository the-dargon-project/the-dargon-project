using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon.Trinkets {
   public class TrinketsImplRyuPackage : RyuPackageV1 {
      public TrinketsImplRyuPackage() {
         PofContext<TrinketsImplPofContext>();
      }
   }
}
