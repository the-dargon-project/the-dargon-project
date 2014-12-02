
using System.Runtime.InteropServices;
using Dargon.Services;

namespace Dargon.InjectedModule {
   [Guid("3ACA63C9-6085-4C87-9B09-E6DBACCBC2D4")]
   public interface InjectedModuleService : IStatusService {
      ISession InjectToProcess(int processId, IInjectedModuleConfiguration configuration);
   }
}
