using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon.LeagueOfLegends {
   public class LeagueOfLegendsApiRyuPackage : RyuPackageV1 {
      public LeagueOfLegendsApiRyuPackage() {
         PofContext<LeagueOfLegendsApiPofContext>();
      }
   }
}
