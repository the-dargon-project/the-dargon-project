using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Trinkets.Hosted {
   /// <summary>
   /// This entry point is only used when running trinket-managed
   /// as a console application. See TrinketProgram entry point
   /// for when we're loaded by trinket-dim.
   /// </summary>
   public unsafe static class DebugProgram {
      public static void Main() {
         TrinketNatives* trinketNatives = stackalloc TrinketNatives[1];
         trinketNatives->startCanary = TrinketNatives.kStartCanary;
         trinketNatives->tailCanary = TrinketNatives.kTailCanary;
         var trinketNativesPointer = (IntPtr)trinketNatives;
         TrinketEntryPoint.TrinketMain(
            $"{trinketNativesPointer.ToInt64()}");
      }
   }
}
