using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImpromptuInterface.InvokeExt;
using ItzWarty;

namespace Dargon.CLI {
   public class DispatcherCommand : ICommand, IDispatcher {
      private readonly IDictionary<string, ICommand> commandsByName = new SortedDictionary<string, ICommand>();
      private readonly string name;
      private IDispatcher parent;

      public DispatcherCommand(string name) {
         this.name = name;
         this.RegisterCommand(new HelpCommand(this, commandsByName));
      }

      public void RegisterCommand(ICommand target) {
         commandsByName.Add(target.Name, target);
      }

      public IDispatcher Parent { get { return parent; } set { parent = value; } }

      public string Name { get { return name; } }
      public string FullName { get { return GetFullName(); } }

      public int Eval(string input) {
         input = input.Trim();
         var inputParts = input.Split(' ');
         var commandName = inputParts[0];
         var subcommand = inputParts.SubArray(1).Join(" ");
         ICommand command;
         if (!commandsByName.TryGetValue(commandName, out command)) {
            throw new CommandDispatchException(FullName, commandName);
         } else {
            return command.Eval(subcommand);
         }
      }

      private string GetFullName() {
         var sb = new StringBuilder();
         IDispatcher current = this;
         while (current != null) {
            sb.Insert(0, current.Name + (current == this ? "" : " "));
            current = current.Parent;
         }
         return sb.ToString().Trim();
      }
   }
}
