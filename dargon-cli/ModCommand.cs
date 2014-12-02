using Dargon.ModificationRepositories;
using Dargon.Services.Client;
using ItzWarty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.CLI {
   public class ModCommand : DispatcherCommand {
      private readonly IServiceClient serviceClient;

      public ModCommand(IServiceClient serviceClient) : base("mod") {
         this.serviceClient = serviceClient;

         RegisterCommand(new ListCommand(serviceClient));
      }

      public class ListCommand : ICommand {
         private readonly IServiceClient serviceClient;

         public ListCommand(IServiceClient serviceClient) {
            this.serviceClient = serviceClient;
         }

         public string Name { get { return "list"; } }

         public int Eval(string subcommand) {
            var repositoryService = serviceClient.GetService<ModificationRepositoryService>();
            var mods = repositoryService.EnumerateModifications().ToArray();
            if (mods.Length == 0) {
               Console.Error.WriteLine("No modifications matched filter.");
            }
            foreach (var mod in mods) {
               var metadata = mod.Metadata;
               Console.WriteLine("{0}({1}) by \"{2}\" for {3}".F(metadata.Name, mod.RepositoryName, metadata.Authors.Join(", "), metadata.Targets.Select(x => x.Name).Join(", ")));
            }
            return 0;
         }
      }
   }
}
