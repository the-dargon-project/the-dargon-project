using System;

namespace Dargon.Processes.Kernel
{
   [Flags]
   public enum AllocationType
   {
      /// <summary>
      ///  Allocates memory charges (from the overall size of memory and the paging files on disk)
      ///     for the specified reserved memory pages. The function also guarantees that when the
      ///     caller later initially accesses the memory, the contents will be zero.  Actual 
      ///     physical pages are not allocated unless/until the virtual addresses are actually
      ///     accessed.
      ///  
      ///  To reserve and commit pages in one step, call VirtualAllocEx with 
      ///     MEM_COMMIT | MEM_RESERVE.
      /// 
      ///  The function fails if you attempt to commit a page that has not been reserved. 
      ///     The resulting error code is ERROR_INVALID_ADDRESS.
      /// 
      ///  An attempt to commit a page that is already committed does not cause the function to
      ///     fail. This means that you can commit pages without first determining the current 
      ///     commitment state of each page.
      /// </summary>
      Commit = 0x1000,

      /// <summary>
      ///  Reserves a range of the process's virtual address space without allocating any actual
      ///     physical storage in memory or in the paging file on disk.
      ///  
      ///  You commit reserved pages by calling VirtualAllocEx again with MEM_COMMIT. To reserve 
      ///     and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
      ///  
      ///  Other memory allocation functions, such as malloc and LocalAlloc, cannot use reserved 
      ///     memory until it has been released.
      /// </summary>
      Reserve = 0x2000,


      Decommit = 0x4000,
      Release = 0x8000,


      /// <summary>
      ///  Indicates that data in the memory range specified by lpAddress and dwSize is no longer
      ///     of interest. The pages should not be read from or written to the paging file.
      ///     However, the memory block will be used again later, so it should not be decommitted. 
      ///     This value cannot be used with any other value.
      /// 
      ///  Using this value does not guarantee that the range operated on with MEM_RESET will
      ///     contain zeros. If you want the range to contain zeros, decommit the memory and then
      ///     recommit it.
      /// 
      ///  MEM_RESET, the VirtualAllocEx function ignores the value of fProtect.  However, you must
      ///     still set fProtect to a valid protection value, such as PAGE_NOACCESS.
      /// 
      ///  VirtualAllocEx returns an error if you use MEM_RESET and the range of memory is mapped
      ///     to a file. A shared view is only acceptable if it is mapped to a paging file.
      /// </summary>
      Reset = 0x80000,
      Physical = 0x400000,
      TopDown = 0x100000,
      WriteWatch = 0x200000,
      LargePages = 0x20000000
   }
}