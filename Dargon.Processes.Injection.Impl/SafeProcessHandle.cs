using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dargon.Processes.Kernel;
using Microsoft.Win32.SafeHandles;

namespace Dargon.Processes.Injection {
   public class SafeProcessHandle : SafeHandleZeroIsInvalid {
      public SafeProcessHandle(
         IntPtr handle, 
         bool ownsHandle = true
      ) : base(handle, ownsHandle) {}

      protected override bool ReleaseValidHandleInternal(IntPtr hProcess) {
         return Kernel32.CloseHandle(hProcess);
      }

      public static SafeProcessHandle OpenOrThrow(int targetProcessId) {
         var hProcess = new SafeProcessHandle(
            Kernel32.OpenProcess(
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
