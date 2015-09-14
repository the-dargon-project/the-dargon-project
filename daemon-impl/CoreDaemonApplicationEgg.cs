using Dargon.Management.Server;
using Dargon.Nest.Egg;
using Dargon.Ryu;
using ItzWarty;
using ItzWarty.Networking;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Dargon.Daemon {
   public class CoreDaemonApplicationEgg : INestApplicationEgg {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const int kDaemonManagementPort = 21001;

      private readonly RyuContainer ryu;

      public CoreDaemonApplicationEgg() {
         ryu = new RyuFactory().Create();
         ((RyuContainerImpl)ryu).SetLoggerEnabled(true);
      }

      public NestResult Start(IEggParameters parameters) {
         InitializeLogging();
         LogIfDebugBuild();

         ryu.Set<IEggHost>(parameters?.Host);

         ryu.Touch<ItzWartyCommonsRyuPackage>();
         ryu.Touch<ItzWartyProxiesRyuPackage>();

         // Dargon.management
         var managementServerEndpoint = ryu.Get<INetworkingProxy>().CreateAnyEndPoint(kDaemonManagementPort);
         ryu.Set<IManagementServerConfiguration>(new ManagementServerConfiguration(managementServerEndpoint));

         ((RyuContainerImpl)ryu).Setup(true);
         
         logger.Info("Initialized.");
         
         return NestResult.Success;
      }

      public NestResult Shutdown() {
         ryu.Get<DaemonService>().Shutdown();
         return NestResult.Success;
      }

      private void InitializeLogging() {
         var config = new LoggingConfiguration();
         Target debuggerTarget = new DebuggerTarget() { 
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };
         Target consoleTarget = new ColoredConsoleTarget() {
            Layout = "${longdate}|${level}|${logger}|${message} ${exception:format=tostring}"
         };

#if !DEBUG
         debuggerTarget = new AsyncTargetWrapper(debuggerTarget);
         consoleTarget = new AsyncTargetWrapper(consoleTarget);
#else
         new AsyncTargetWrapper().Wrap(); // Placeholder for optimizing imports
#endif

         config.AddTarget("debugger", debuggerTarget);
         config.AddTarget("console", consoleTarget);

         var debuggerRule = new LoggingRule("*", LogLevel.Trace, debuggerTarget);
         config.LoggingRules.Add(debuggerRule);

         var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
         config.LoggingRules.Add(consoleRule);

         LogManager.Configuration = config;
      }

      private void LogIfDebugBuild() {
#if DEBUG
         logger.Error("COMPILED IN DEBUG MODE");
#endif
      }
   }
}
