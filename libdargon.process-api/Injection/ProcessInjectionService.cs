namespace Dargon.Processes.Injection
{
   public interface ProcessInjectionService
   {
      bool InjectToProcess(int processId, string dllPath);
   }
}