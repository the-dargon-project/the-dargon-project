using Dargon.InjectedModule;

namespace Dargon.FinalFantasyXIII.Lifecycle
{
   public interface IFFXIIIInjectedModuleConfigurationFactory
   {
      IInjectedModuleConfiguration GetLauncherConfiguration();
      IInjectedModuleConfiguration GetGameConfiguration();
   }
}
