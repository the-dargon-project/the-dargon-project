namespace Dargon.Processes.Injection
{
   public interface ProcessInjectionService
   {
      bool InjectToProcess(int processId, string dllPath, int attempts = 100, int attemptInterval = 200);
   }
}