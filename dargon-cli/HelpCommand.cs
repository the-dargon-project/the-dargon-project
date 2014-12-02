using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;

namespace Dargon.CLI {
   public class HelpCommand : ICommand {
      private readonly IDispatcher dispatcher;
      private readonly IDictionary<string, ICommand> commandsByName;

      public HelpCommand(IDispatcher dispatcher, IDictionary<string, ICommand> commandsByName) {
         this.dispatcher = dispatcher;
         this.commandsByName = commandsByName;
      }

      public string Name { get { return "help"; } }

      public int Eval(string subcommand) {
         subcommand = subcommand.Trim();
         var inputParts = subcommand.Split(" ", StringSplitOptions.RemoveEmptyEntries);

         if (inputParts.Length > 0) {
            if (inputParts[0].Equals(Name)) {
               Console.WriteLine("Tricky.");
               return 0;
            } else {
               return dispatcher.Eval(inputParts.Concat(Name).Join(" "));
            }
         } else {
            foreach (var kvp in this.commandsByName) {
               Console.WriteLine(kvp.Key);
            }
            return 0;
         }
      }
   }
}
