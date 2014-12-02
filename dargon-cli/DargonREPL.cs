
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dargon.CLI;
using Dargon.Services.Client;
using ItzWarty;

namespace Dargon.CLI {
   public class DargonREPL {
      private readonly IDispatcher dispatcher;

      public DargonREPL(IDispatcher dispatcher) {
         this.dispatcher = dispatcher;
      }

      public void Run() {
         while (true) {
            Console.Write("$ ");
            var input = Console.ReadLine();
            try {
               dispatcher.Eval(input);
            } catch (CommandDispatchException e) {
               Console.WriteLine(e.Message);
            }
            Console.WriteLine();
         }
      }
   }
}
