using ItzWarty.Services;
using NLog;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionServiceImpl : ProcessInjectionService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IProcessInjector injector;

      public ProcessInjectionServiceImpl(IServiceLocator serviceLocator, IProcessInjector injector)
      {
         logger.Info("Initializing Process Injection Service");

         serviceLocator.RegisterService(typeof(ProcessInjectionService), this);

         this.injector = injector;
      }

      public bool InjectToProcess(int processId, string dllPath, int attempts = 100, int attemptInterval = 200)
      {
         return injector.TryInject(processId, dllPath, attempts, attemptInterval);
      }
   }
}
