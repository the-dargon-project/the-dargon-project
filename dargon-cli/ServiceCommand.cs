using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.InjectedModule;
using Dargon.Services.Client;
using ItzWarty;

namespace Dargon.CLI {
   public class ServiceCommand : DispatcherCommand {
      private const string kCommandName = "service";

      public ServiceCommand(IServiceClient serviceClient) : base(kCommandName) {
         RegisterCommand(new InjectedModuleServiceCommand(serviceClient));
      }

      public override int Eval(string input) {
         if (string.IsNullOrWhiteSpace(input.Trim()) || input.Trim().Equals("status")) {
            foreach (var kvp in CommandsByName) {
               if (kvp.Key != HelpCommand.kCommandName) {
                  Console.WriteLine("service \"{0}\" status:".F(kvp.Key));
                  kvp.Value.Eval("status");
               }
            }
            return 0;
         } else {
            return base.Eval(input);
         }
      }

      public class InjectedModuleServiceCommand : DispatcherCommand {
         private const string kCommandName = "injected-module";

         public InjectedModuleServiceCommand(IServiceClient serviceClient) : base(kCommandName) {
            RegisterCommand(new StatusCommand(serviceClient));
         }

         public class StatusCommand : ICommand {
            private readonly IServiceClient serviceClient;

            public StatusCommand(IServiceClient serviceClient) { this.serviceClient = serviceClient; }

            public string Name { get { return "status"; } }

            public int Eval(string subcommand) {
               var service = serviceClient.GetService<InjectedModuleService>();
               Console.WriteLine(service.GetStatus().Trim());
               return 0;
            }
         }
      }
   }
}
