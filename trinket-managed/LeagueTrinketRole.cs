using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Trinkets.Hosted {
   public enum LeagueTrinketRole {
      Air,
      Game,
      Unknown
   }

   public static class LeagueTrinketRoleUtilities {
      public static LeagueTrinketRole GetRole() {
         var processExeName = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
         if (processExeName.Name.Equals("LolClient.exe", StringComparison.OrdinalIgnoreCase)) {
            return LeagueTrinketRole.Air;
         } else if (processExeName.Name.Equals("league of legends.exe", StringComparison.OrdinalIgnoreCase)) {
            return LeagueTrinketRole.Game;
         } else {
            return LeagueTrinketRole.Unknown;
         }
      }
   }
}
