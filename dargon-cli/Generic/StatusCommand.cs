using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.InjectedModule;
using Dargon.ManageableServices;
using Dargon.Services;
using Dargon.Services.Client;

namespace Dargon.CLI.Generic {
   public class StatusCommand<TService> : ICommand
      where TService : class, IStatusService {
      private readonly IServiceClient serviceClient;

      public StatusCommand(IServiceClient serviceClient) { this.serviceClient = serviceClient; }

      public string Name { get { return "status"; } }

      public int Eval(string subcommand) {
         var service = serviceClient.GetService<TService>();
         Console.WriteLine(service.GetStatus().Trim());
         return 0;
      }
   }
}
