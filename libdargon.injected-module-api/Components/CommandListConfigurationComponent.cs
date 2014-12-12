using Dargon.InjectedModule.Commands;

namespace Dargon.InjectedModule.Components
{
   public class CommandListConfigurationComponent : IConfigurationComponent
   {
      private const string kEnableCommandListFlag = "--enable-dim-command-list";
      
      private readonly ICommandList commandList;

      public CommandListConfigurationComponent(ICommandList commandList) { this.commandList = commandList; }

      public ICommandList CommandList { get { return commandList; } }

      public void AmendBootstrapConfiguration(BootstrapConfigurationBuilder builder) { builder.SetFlag(kEnableCommandListFlag); }
   }
}
