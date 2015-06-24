using NLog;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dargon.Processes.Injection {
   public class SafeRemoteThreadHandle : SafeHandleZeroIsInvalid {
      private readonly static Logger logger = LogManager.GetCurrentClassLogger();
      private readonly static UIntPtr pLoadLibraryA;

      static SafeRemoteThreadHandle() {
         var hKernel32 = WinAPI.GetModuleHandle("kernel32.dll");
         pLoadLibraryA = WinAPI.GetProcAddress(hKernel32, "LoadLibraryA");
      }

      public SafeRemoteThreadHandle(IntPtr handle, bool ownsHandle = true) : base(handle, ownsHandle) {}

      protected override bool ReleaseValidHandleInternal(IntPtr hThread) {
         return WinAPI.CloseHandle(hThread);
      }

      public bool TryWaitForTermination(int timeoutMilliseconds) {
         var waitResult = WinAPI.WaitForSingleObject(handle, timeoutMilliseconds);
         logger.Info($"The wait result is: {waitResult}");
         if (waitResult == WaitForSingleObjectResult.WAIT_OBJECT_0) {
            return true;
         } else {
            if (waitResult == WaitForSingleObjectResult.WAIT_FAILED) {
               logger.Error($"Errno: {Marshal.GetLastWin32Error()} for handle {handle}.");
            }
            return false;
         }
      }

      public static SafeRemoteThreadHandle StartRemoteDllThreadOrThrow(SafeProcessHandle hRemoteProcess, SafeRemoteBufferHandle hRemoteDllPathBuffer, int attempts) {
         for (var i = 0; i < attempts; i++) {
            SafeRemoteThreadHandle result;
            if (TryStartRemoteDllThreadHelper(hRemoteProcess, hRemoteDllPathBuffer, out result)) {
               return result;
            }
         }

         throw new Exception(
            "Could not create remote thread after " + attempts + " attempts!\n" +
            "\r\nhProcess: " + hRemoteProcess.DangerousGetHandle() +
            "\r\nhRemoteDllPathBuffer: " + hRemoteDllPathBuffer.DangerousGetHandle() +
            "\r\npLoadLibraryA: " + pLoadLibraryA +
            "\r\nErrno: " + Marshal.GetLastWin32Error()
         );
      }

      private static bool TryStartRemoteDllThreadHelper(SafeProcessHandle hRemoteProcess, SafeRemoteBufferHandle hRemoteDllPathBuffer, out SafeRemoteThreadHandle hRemoteThreadOut) {
         var hProcessUnsafe = hRemoteProcess.DangerousGetHandle();
         var pRemoteDllPath = hRemoteDllPathBuffer.DangerousGetHandle();

         try {
            uint remoteThreadId;
            var hRemoteThread = WinAPI.CreateRemoteThread(
               hProcessUnsafe,
               IntPtr.Zero,
               0,
               pLoadLibraryA,
               pRemoteDllPath,
               0,
               out remoteThreadId
            );
            if (hRemoteThread == IntPtr.Zero) {
               logger.Warn($"CreateRemoteThread failed with errno {Marshal.GetLastWin32Error()}.");
               hRemoteThreadOut = null;
            } else {
               hRemoteThreadOut = new SafeRemoteThreadHandle(hRemoteThread);
            }
         } catch (Win32Exception e) {
            var errno = Marshal.GetLastWin32Error();
            logger.Warn("Win32Exception thrown when creating remote thread. Errno: " + errno + ".", e);
            hRemoteThreadOut = null;
         }

         return hRemoteThreadOut != null;
      }
   }
}
