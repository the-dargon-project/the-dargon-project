using Dargon.Daemon;
using Dargon.Services;
using System;

namespace Dargon.CLI {
   public class ShutdownCommand : ICommand {
      private readonly ServiceClient serviceClient;

      public ShutdownCommand(ServiceClient serviceClient) {
         this.serviceClient = serviceClient;
      }

      public string Name { get { return "shutdown"; } }

      public int Eval(string subcommand) {
         Console.Write("Sending shutdown signal to Daemon Service... ");
         var daemonService = serviceClient.GetService<DaemonService>();
         daemonService.Shutdown();
         Console.WriteLine(daemonService.IsShutdownSignalled ? "success" : "failure");
         return 0;
      }
   }
}
