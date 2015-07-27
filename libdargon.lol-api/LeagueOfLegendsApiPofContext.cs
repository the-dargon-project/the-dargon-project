using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.PortableObjects;

namespace Dargon.LeagueOfLegends {
   public class LeagueOfLegendsApiPofContext : PofContext {
      private const int kBasePofId = 12000;
      public LeagueOfLegendsApiPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(LeagueModificationCategory));
      }
   }
}
