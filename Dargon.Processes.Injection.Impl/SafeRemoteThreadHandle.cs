using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.Processes.Kernel;
using NLog;

namespace Dargon.Processes.Injection {
   public class SafeRemoteThreadHandle : SafeHandleZeroIsInvalid {
      private readonly static Logger logger = LogManager.GetCurrentClassLogger();
      private readonly static UIntPtr pLoadLibraryA;

      static SafeRemoteThreadHandle() {

      }

      public SafeRemoteThreadHandle(IntPtr handle, bool ownsHandle = true) : base(handle, ownsHandle) {}

      protected override bool ReleaseValidHandleInternal(IntPtr hThread) {
         return Kernel32.CloseHandle(hThread);
      }

      public bool TryWaitForTermination(int timeoutMilliseconds) {
         var waitResult = Kernel32.WaitForSingleObject(handle, timeoutMilliseconds);
         if (waitResult == WaitForSingleObjectResult.WAIT_OBJECT_0) {
            return true;
         } else {
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
            var hRemoteThread = Kernel32.CreateRemoteThread(
               hProcessUnsafe,
               IntPtr.Zero,
               0,
               pLoadLibraryA,
               pRemoteDllPath,
               0,
               out remoteThreadId
            );
            hRemoteThreadOut = new SafeRemoteThreadHandle(hRemoteThread);
         } catch (Win32Exception e) {
            var errno = Marshal.GetLastWin32Error();
            logger.Warn("Win32Exception thrown when creating remote thread. Errno: " + errno + ".", e);
            hRemoteThreadOut = null;
         }

         return hRemoteThreadOut == null;
      }
   }
}
