using Dargon.Processes.Injection;
using ItzWarty.Services;
using NLog;

namespace Dargon.InjectedModule
{
   public class InjectedModuleServiceImpl : InjectedModuleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessInjectionService processInjectionService;

      public InjectedModuleServiceImpl(IServiceLocator serviceLocator, ProcessInjectionService processInjectionService) {
         logger.Info("Initializing Injected Module Service");

         serviceLocator.RegisterService(typeof(InjectedModuleService), this);

         this.processInjectionService = processInjectionService;
      }

      public void InjectToProcess(int processId, BootstrapConfiguration bootstrapConfiguration) 
      { 

      }
   }
}
