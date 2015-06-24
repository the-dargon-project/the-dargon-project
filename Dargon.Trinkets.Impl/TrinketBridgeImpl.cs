using System;
using Dargon.Processes.Injection;
using Dargon.Trinkets.Transport;
using NLog;

namespace Dargon.Trinkets {
   public class TrinketBridgeImpl : TrinketBridge {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ProcessInjectionService processInjectionService;
      private readonly TrinketInternalUtilities trinketInternalUtilities;
      private readonly TrinketStartupConfiguration configuration;
      private readonly TrinketDtpServer trinketDtpServer;

      public TrinketBridgeImpl(ProcessInjectionService processInjectionService, TrinketInternalUtilities trinketInternalUtilities, TrinketStartupConfiguration configuration, TrinketDtpServer trinketDtpServer) {
         this.processInjectionService = processInjectionService;
         this.trinketInternalUtilities = trinketInternalUtilities;
         this.configuration = configuration;
         this.trinketDtpServer = trinketDtpServer;
      }

      public bool Initialize() {
         Console.WriteLine("!");
         logger.Info("Initializing Trinket Bridge");
         var dimPath = trinketInternalUtilities.GetInjectedModulePath();
         logger.Info($"Dim Path: {dimPath}");

         var injectionSuccessful = processInjectionService.InjectToProcess(configuration.TargetProcessId, dimPath);
         logger.Info($"Injection Successful: {injectionSuccessful}");

         return injectionSuccessful;
      }
   }
}