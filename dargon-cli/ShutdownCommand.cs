using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Daemon;
using Dargon.Services.Client;

namespace Dargon.CLI {
   public class ShutdownCommand : ICommand {
      private readonly IServiceClient serviceClient;

      public ShutdownCommand(IServiceClient serviceClient) {
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
