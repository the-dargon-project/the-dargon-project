using ItzWarty;
using NLog;
using System.Text;

namespace Dargon.Processes.Injection {
   public class ProcessInjectionServiceImpl : ProcessInjectionService {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessInjector injector;
      private readonly ProcessInjectionConfiguration configuration;

      public ProcessInjectionServiceImpl(ProcessInjector injector, ProcessInjectionConfiguration configuration) {
         logger.Info("Initializing Process Injection Service");

         this.injector = injector;
         this.configuration = configuration;
      }

      public ProcessInjectionConfiguration Configuration => configuration;

      public bool InjectToProcess(int processId, string dllPath) {
         return injector.TryInjectToProcess(processId, dllPath, configuration.InjectionAttempts, configuration.InjectionAttemptDelay) == ProcessInjectionResult.Success;
      }

      public string GetStatus() {
         var sb = new StringBuilder();
         sb.AppendLine("Configuration: {0} attempts with {1}ms delay.".F(configuration.InjectionAttempts, configuration.InjectionAttemptDelay));
         return sb.ToString();
      }
   }
}
