using System;
using System.Runtime.InteropServices;
using System.Threading;
using Dargon.Processes.Kernel;

namespace Dargon.Processes.Injection {
   public abstract class SafeHandleZeroIsInvalid : SafeHandle {
      protected SafeHandleZeroIsInvalid(
         IntPtr handle, 
         bool ownsHandle = true
      ) : base(IntPtr.Zero, ownsHandle) {
         this.handle = handle;
      }

      public override bool IsInvalid => handle == IntPtr.Zero;

      protected override bool ReleaseHandle() {
         var handleCapture = Interlocked.CompareExchange(ref handle, IntPtr.Zero, handle);
         if (handleCapture != IntPtr.Zero) {
            return ReleaseValidHandleInternal(handleCapture);
         }
         return true;
      }

      protected abstract bool ReleaseValidHandleInternal(IntPtr handle);
   }
}