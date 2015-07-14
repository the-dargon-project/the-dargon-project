using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Manager {
   public static class Program {
      public static void Main() {
         Console.WriteLine("Entered Main");
         new DargonManagerApplicationEgg().Start(null);
      }
   }
}
