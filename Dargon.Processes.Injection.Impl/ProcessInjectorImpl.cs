using NLog;
using System;
using System.IO;
using System.Threading;

namespace Dargon.Processes.Injection {
   /// <summary>
   /// Handles injection of dynamically linked libraries to remote processes
   /// </summary>
   public class ProcessInjectorImpl : ProcessInjector
   {
      private readonly static Logger logger = LogManager.GetCurrentClassLogger();

      private const int kDllCompletionTimeoutMilliseconds = 15000;
      
      public ProcessInjectionResult InjectToProcessOrThrow(int targetProcessId, string dllPath)
      {
         // Normalize dll path so it is properly loaded in target process
         dllPath = Path.GetFullPath(dllPath);

         logger.Info("Injecting into processId " + targetProcessId + " dll " + dllPath);
         using (var hProcess = SafeProcessHandle.OpenOrThrow(targetProcessId))
         using (var hDllPathBuffer = SafeRemoteBufferHandle.AllocateOrThrow(hProcess, dllPath))
         using (var hRemoteThread = SafeRemoteThreadHandle.StartRemoteDllThreadOrThrow(hProcess, hDllPathBuffer, 10)) {
            if (hRemoteThread.TryWaitForTermination(kDllCompletionTimeoutMilliseconds)) {
               return ProcessInjectionResult.Success;
            } else {
               return ProcessInjectionResult.DllFailed;
            }
         }
      }

      public ProcessInjectionResult TryInjectToProcess(int targetProcessId, string dllPath, int attempts, int attemptsIntervalMilliseconds) {
         for (var i = 0; i < attempts; i++) {
            try {
               return InjectToProcessOrThrow(targetProcessId, dllPath);
            } catch (Exception e) {
               logger.Info("InjectToProcess attempt failed. ", e);
               Thread.Sleep(attemptsIntervalMilliseconds);
            }
         }
         return ProcessInjectionResult.InjectionFailed;
      }
   }
}
