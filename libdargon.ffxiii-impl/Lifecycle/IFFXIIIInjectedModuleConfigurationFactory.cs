using Dargon.InjectedModule;
using Dargon.InjectedModule.Tasks;

namespace Dargon.FinalFantasyXIII.Lifecycle
{
   public interface IFFXIIIInjectedModuleConfigurationFactory
   {
      IInjectedModuleConfiguration GetLauncherConfiguration();
      IInjectedModuleConfiguration GetGameConfiguration();
   }
}
