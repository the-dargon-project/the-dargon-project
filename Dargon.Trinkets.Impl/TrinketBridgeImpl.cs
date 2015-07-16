using System;
using System.IO;
using Dargon.Processes.Injection;
using Dargon.Trinkets.Transport;
using NLog;

namespace Dargon.Trinkets {
   public class TrinketBridgeImpl : TrinketBridge {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly TemporaryFileService temporaryFileService;
      private readonly ProcessInjectionService processInjectionService;
      private readonly TrinketInternalUtilities trinketInternalUtilities;
      private readonly TrinketStartupConfiguration configuration;
      private readonly TrinketDtpServer trinketDtpServer;

      public TrinketBridgeImpl(TemporaryFileService temporaryFileService, ProcessInjectionService processInjectionService, TrinketInternalUtilities trinketInternalUtilities, TrinketStartupConfiguration configuration, TrinketDtpServer trinketDtpServer) {
         this.temporaryFileService = temporaryFileService;
         this.processInjectionService = processInjectionService;
         this.trinketInternalUtilities = trinketInternalUtilities;
         this.configuration = configuration;
         this.trinketDtpServer = trinketDtpServer;
      }

      public bool Initialize() {
         Console.WriteLine("!");
         logger.Info("Initializing Trinket Bridge");
         var originalDimPath = trinketInternalUtilities.GetInjectedModulePath();
         logger.Info($"Original Dim Path: {originalDimPath}");
         var temporaryDirectory = temporaryFileService.AllocateTemporaryDirectory(TimeSpan.FromMinutes(1));
         foreach (var file in new FileInfo(originalDimPath).Directory.EnumerateFiles("*", SearchOption.TopDirectoryOnly)) {
            file.CopyTo(Path.Combine(temporaryDirectory, file.Name));
         }
         var clonedDimPath = Path.Combine(temporaryDirectory, new FileInfo(originalDimPath).Name);
         logger.Info($"Cloned Dim Path: {clonedDimPath}");

         var injectionSuccessful = processInjectionService.InjectToProcess(configuration.TargetProcessId, clonedDimPath);
         logger.Info($"Injection Successful?: {injectionSuccessful}");

         return injectionSuccessful;
      }
   }
}