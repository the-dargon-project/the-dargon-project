namespace Dargon.Processes.Kernel
{
   public enum WaitForSingleObjectResult : uint
   {
      /// <summary>
      /// The state of the specified object is signaled.
      /// In the case of a process or thread, that means the thing has reached
      ///    the end of execution.
      /// </summary>
      WAIT_OBJECT_0     = 0x00000000U,

      /// <summary>
      /// The specified object is a mutex object that was not released by the thread that owned 
      /// the mutex object before the owning thread terminated. Ownership of the mutex object is 
      /// granted to the calling thread and the mutex state is set to nonsignaled.
      /// 
      /// If the mutex was protecting persistent state information, you should check it for 
      /// consistency.
      /// </summary>
      WAIT_ABANDONED    = 0x00000080U,

      /// <summary>
      /// The time-out interval elapsed, and the object's state is nonsignaled.
      /// </summary>
      WAIT_TIMEOUT      = 0x00000102U,

      /// <summary>
      /// The function has failed. To get extended error information, call GetLastError.
      /// </summary>
      WAIT_FAILED       = 0xFFFFFFFFU
   }
}
