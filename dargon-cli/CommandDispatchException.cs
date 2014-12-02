using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.CLI {
   public class CommandDispatchException : Exception {
      private readonly string source;
      private readonly string command;

      public CommandDispatchException(string source, string command) 
         : base("Unable to find command '" + command + "' in " + source){
         this.source = source;
         this.command = command;
      }

      public string Source { get { return source; } }
      public string Command { get { return command; } }
   }
}
