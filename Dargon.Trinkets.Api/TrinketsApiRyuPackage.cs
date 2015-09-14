using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;
using Dargon.Trinkets.Commands;

namespace Dargon.Trinkets {
   public class TrinketsApiRyuPackage : RyuPackageV1 {
      public TrinketsApiRyuPackage() {
         Singleton<CommandFactory, CommandFactoryImpl>();
         PofContext<TrinketsApiPofContext>();
      }
   }
}
