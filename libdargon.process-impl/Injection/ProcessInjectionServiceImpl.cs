using ItzWarty.Services;
using NLog;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionServiceImpl : ProcessInjectionService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IProcessInjector injector;
      private readonly IProcessInjectionConfiguration configuration;

      public ProcessInjectionServiceImpl(IServiceLocator serviceLocator, IProcessInjector injector, IProcessInjectionConfiguration configuration)
      {
         logger.Info("Initializing Process Injection Service");

         serviceLocator.RegisterService(typeof(ProcessInjectionService), this);

         this.injector = injector;
         this.configuration = configuration;
      }

      public bool InjectToProcess(int processId, string dllPath)
      {
         return injector.TryInject(processId, dllPath, configuration.InjectionAttempts, configuration.InjectionAttemptDelay);
      }
   }
}
