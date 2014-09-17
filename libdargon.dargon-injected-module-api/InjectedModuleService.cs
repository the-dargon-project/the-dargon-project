using System.Data;

namespace Dargon.InjectedModule
{
   public interface InjectedModuleService
   {
      void InjectToProcess(int processId, BootstrapConfiguration bootstrapConfiguration);
   }
}
