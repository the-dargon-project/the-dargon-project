using System.Runtime.InteropServices;
using Dargon.Services;

namespace Dargon.Processes.Injection {
   [Guid("9AD158F8-C98E-405E-96DD-8D24424BE38B")]
   public interface ProcessInjectionService : IStatusService {
      IProcessInjectionConfiguration Configuration { get; }
      bool InjectToProcess(int processId, string dllPath);
   }
}