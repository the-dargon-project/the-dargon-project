using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Game.LeagueOfLegends
{
   public enum LeagueProcessType
   {
      Invalid = -1,

      GameClient,
      PvpNetClient,
      Launcher,
      RadsUserKernel,
      BugSplat,

      Count
   }
}
