using System.Text;
using ItzWarty;
using NLog;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionServiceImpl : ProcessInjectionService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IProcessInjector injector;
      private readonly IProcessInjectionConfiguration configuration;

      public ProcessInjectionServiceImpl(IProcessInjector injector, IProcessInjectionConfiguration configuration)
      {
         logger.Info("Initializing Process Injection Service");

         this.injector = injector;
         this.configuration = configuration;
      }

      public IProcessInjectionConfiguration Configuration { get { return configuration; } }

      public bool InjectToProcess(int processId, string dllPath)
      {
         return injector.TryInject(processId, dllPath, configuration.InjectionAttempts, configuration.InjectionAttemptDelay);
      }

      public string GetStatus() {
         var sb = new StringBuilder();
         sb.AppendLine("Configuration: {0} attempts with {1}ms delay.".F(configuration.InjectionAttempts, configuration.InjectionAttemptDelay));
         return sb.ToString();
      }
   }
}
