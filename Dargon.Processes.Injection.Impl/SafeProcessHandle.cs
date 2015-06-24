using System;
using System.Runtime.InteropServices;

namespace Dargon.Processes.Injection {
   public class SafeProcessHandle : SafeHandleZeroIsInvalid {
      public SafeProcessHandle(
         IntPtr handle, 
         bool ownsHandle = true
      ) : base(handle, ownsHandle) {}

      protected override bool ReleaseValidHandleInternal(IntPtr hProcess) {
         return WinAPI.CloseHandle(hProcess);
      }

      public static SafeProcessHandle OpenOrThrow(int targetProcessId) {
         var hProcess = new SafeProcessHandle(
            WinAPI.OpenProcess(
               ProcessAccessRights.PROCESS_ALL_ACCESS,
               1 /* We do want to own the handle */,
               targetProcessId
            )
         );

         if (hProcess.IsInvalid) {
            throw new UnauthorizedAccessException(
               "Could acquire target process handle!\n" +
               "OpenProcess returned invalid Handle! Is the Process Injector running with elevated privileges?" +
               "\nProcess Id: " + targetProcessId +
               "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }

         return hProcess;
      }
   }
}
