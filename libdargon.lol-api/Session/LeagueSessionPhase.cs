using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.LeagueOfLegends.Session
{
   public enum LeagueSessionPhase
   {
      Uninitialized,
      Preclient,
      Client,
      Game,
      Quit
   }
}
