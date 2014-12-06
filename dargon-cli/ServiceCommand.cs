﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.CLI.Generic;
using Dargon.CLI.Interface;
using Dargon.InjectedModule;
using Dargon.Processes.Injection;
using Dargon.Services.Client;
using ItzWarty;

namespace Dargon.CLI {
   public class ServiceCommand : DispatcherCommand {
      private const string kCommandName = "service";

      public ServiceCommand(IServiceClient serviceClient) : base(kCommandName) {
         RegisterCommand(new InjectedModuleServiceCommand(serviceClient));
         RegisterCommand(new ProcessInjectionServiceCommand(serviceClient));
      }

      public override int Eval(string input) {
         if (string.IsNullOrWhiteSpace(input.Trim()) || input.Trim().Equals("status")) {
            bool first = true;
            foreach (var kvp in CommandsByName) {

               if (kvp.Key != HelpCommand.kCommandName) {
                  if (first) {
                     first = false;
                  } else {
                     Console.WriteLine();
                  }
                  Console.WriteLine("service \"{0}\" status:".F(kvp.Key));
                  kvp.Value.Eval("status");
               }
            }
            return 0;
         } else if (input.Trim().Equals("list")) { 
            new ListView(CommandsByName.Select(kvp => kvp.Key).Where(k => k != HelpCommand.kCommandName)).PrintToConsole();
            return 0;
         } else {
            return base.Eval(input);
         }
      }

      public class InjectedModuleServiceCommand : DispatcherCommand {
         private const string kCommandName = "injected-module";

         public InjectedModuleServiceCommand(IServiceClient serviceClient) : base(kCommandName) {
            RegisterCommand(new StatusCommand<InjectedModuleService>(serviceClient));
         }
      }

      public class ProcessInjectionServiceCommand : DispatcherCommand {
         private const string kCommandName = "process-injection";

         public ProcessInjectionServiceCommand(IServiceClient serviceClient) : base(kCommandName) {
            RegisterCommand(new StatusCommand<ProcessInjectionService>(serviceClient));
         }
      }
   }
}