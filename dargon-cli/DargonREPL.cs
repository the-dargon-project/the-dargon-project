
using System;
using Dargon.CLI;
using Dargon.Services.Client;

namespace Dargon.CLI {
   public class DargonREPL {
      private readonly IServiceClient serviceClient;

      public DargonREPL(IServiceClient serviceClient) {
         this.serviceClient = serviceClient;
      }

      public void Run() {
         while (true) {
            Console.Write("$ ");
            var command = Console.ReadLine();
         }
      }
   }
}
