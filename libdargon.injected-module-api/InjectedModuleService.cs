
using System.Runtime.InteropServices;

namespace Dargon.InjectedModule
{
   [Guid("3ACA63C9-6085-4C87-9B09-E6DBACCBC2D4")]
   public interface InjectedModuleService
   {
      ISession InjectToProcess(int processId, IInjectedModuleConfiguration configuration);

      // Gets Injected Module Service status for cli command
      string GetStatus();
   }
}
