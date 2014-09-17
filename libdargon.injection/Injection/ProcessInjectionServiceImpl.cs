using ItzWarty.Services;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionServiceImpl : ProcessInjectionService
   {
      private readonly ProcessInjector injector = new ProcessInjector();

      public ProcessInjectionServiceImpl(IServiceLocator serviceLocator)
      {
         serviceLocator.RegisterService(typeof(ProcessInjectionService), this);
      }

      public bool InjectToProcess(int processId, string dllPath, int attempts = 100, int attemptInterval = 200)
      {
         return injector.TryInject(processId, dllPath, attempts, attemptInterval);
      }
   }
}
