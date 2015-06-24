using ItzWarty;
using NLog;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace Dargon.InjectedModule {
   public class InjectedModuleServiceImpl : InjectedModuleService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessInjectionService processInjectionService;
      private readonly ISessionFactory sessionFactory;
      private readonly IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration;
      private readonly ConcurrentDictionary<int, ISession> sessionsByProcessId = new ConcurrentDictionary<int, ISession>();

      public InjectedModuleServiceImpl(ProcessInjectionService processInjectionService, ISessionFactory sessionFactory, IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration)
      {
         logger.Info("Initializing Injected Module Service");

         this.processInjectionService = processInjectionService;
         this.sessionFactory = sessionFactory;
         this.injectedModuleServiceConfiguration = injectedModuleServiceConfiguration;
      }

      public void Initialize()
      {
         var injectedDllPath = injectedModuleServiceConfiguration.GetInjectedDllPath();
         if (!File.Exists(injectedDllPath)) {
            logger.Warn("Injected DLL does not exist at " + injectedDllPath + "!");
         }
      }

      public ISession InjectToProcess(int processId, DimInstanceContext instanceContext) 
      {
         logger.Info("Injecting to process " + processId);

         var session = sessionFactory.CreateSession(processId, instanceContext);
         processInjectionService.InjectToProcess(processId, injectedModuleServiceConfiguration.GetInjectedDllPath());
         sessionsByProcessId.AddOrUpdate(processId, session, (a, b) => session);
         session.Ended += HandleSessionEnded;
         return session;
      }

      public string GetStatus() {
         StringBuilder sb = new StringBuilder();
         var injectedModulePath = injectedModuleServiceConfiguration.GetInjectedDllPath();
         var injectedModuleExists = File.Exists(injectedModulePath) ? "(exists)" : "(not found)";
         sb.AppendLine("Injected Module Path: " + injectedModulePath + " " + injectedModuleExists);
         sb.AppendLine("Sessions:");
         var sessions = sessionsByProcessId.ToArray();
         if (sessions.Length == 0) {
            sb.AppendLine("(none)");
         } else {
            const int kPidColumnLength = 6;
            sb.AppendLine("PID".PadLeft(kPidColumnLength) + " Configuration");
            foreach (var session in sessions) {
               var configuration = session.Value.Configuration.GetBootstrapConfiguration();
               var configurationString = configuration.Flags.Concat(configuration.Properties.Select(kvp => kvp.Key + "=" + kvp.Value)).Join(" ");
               sb.AppendLine(session.Key.ToString().PadLeft(kPidColumnLength) + " " + configurationString);
            }
         }
         return sb.ToString();
      }

      internal void HandleSessionEnded(ISession session, SessionEndedEventArgs sessionEndedEventArgs)
      {
         session.Ended -= HandleSessionEnded;

         ISession removedSession;
         sessionsByProcessId.TryRemove(session.ProcessId, out removedSession);
      }
   }
}
