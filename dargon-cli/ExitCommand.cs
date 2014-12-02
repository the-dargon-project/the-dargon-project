using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.CLI {
   public class ExitCommand : ICommand {
      public string Name { get { return "exit"; } }

      public int Eval(string subcommand) {
         Environment.Exit(0);
         return 0;
      }
   }
}
