using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Trinkets.Commands;

namespace Dargon.Trinkets {
   public class TrinketsImplPofContext : PofContext {
      private const int kBasePofId = 11500;

      public TrinketsImplPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(DefaultCommandList));
      }
   }
}
