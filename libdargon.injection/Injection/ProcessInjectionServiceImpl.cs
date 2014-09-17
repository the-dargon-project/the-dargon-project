using ItzWarty.Services;
using NLog;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionServiceImpl : ProcessInjectionService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessInjector injector = new ProcessInjector();

      public ProcessInjectionServiceImpl(IServiceLocator serviceLocator)
      {
         logger.Info("Initializing Process Injection Service");
         serviceLocator.RegisterService(typeof(ProcessInjectionService), this);
      }

      public bool InjectToProcess(int processId, string dllPath, int attempts = 100, int attemptInterval = 200)
      {
         return injector.TryInject(processId, dllPath, attempts, attemptInterval);
      }
   }
}
