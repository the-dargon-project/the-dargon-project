using Dargon.ManageableServices;
using System.Runtime.InteropServices;

namespace Dargon.Processes.Injection {
   [Guid("9AD158F8-C98E-405E-96DD-8D24424BE38B")]
   public interface ProcessInjectionService : IStatusService {
      ProcessInjectionConfiguration Configuration { get; }
      bool InjectToProcess(int processId, string dllPath);
   }
}