
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dargon.CLI;
using Dargon.Services.Client;
using ItzWarty;

namespace Dargon.CLI {
   public class DargonREPL {
      private readonly IServiceClient serviceClient;
      private readonly IDictionary<string, ICommand> commandsByName = new Dictionary<string, ICommand>();

      public DargonREPL(IServiceClient serviceClient) {
         this.serviceClient = serviceClient;
      }

      public void RegisterCommandTarget(ICommand target) {
         commandsByName.Add(target.Name, target);
      }

      public void Run() {
         while (true) {
            Console.Write("$ ");
            var input = Console.ReadLine();
            var inputParts = input.Split(' ');
            var commandName = inputParts[0];
            var subcommand = inputParts.SubArray(1).Join(" ");
            ICommand command;
            if (!commandsByName.TryGetValue(commandName, out command)) {
               Console.WriteLine("Unknown command " + commandName + ".");
            } else {
               command.Eval(subcommand);
            }
            Console.WriteLine();
         }
      }
   }
}
