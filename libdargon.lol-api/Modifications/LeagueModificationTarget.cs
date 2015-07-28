using System;

namespace Dargon.LeagueOfLegends.Modifications
{
   [Flags]
   public enum LeagueModificationTarget : uint
   {
      Invalid  = 0,
      Client   = 0x00000001U,
      Game     = 0x00000002U
   }
}
