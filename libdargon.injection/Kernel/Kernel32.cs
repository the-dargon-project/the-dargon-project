//Most stuff pulled from pinvoke.net

using System;
using System.Runtime.InteropServices;

namespace Dargon.Processes.Kernel
{
   public static class Kernel32
   {
      /// <summary>
      ///  Reserves or commits a region of memory within the virtual address space of a specified 
      ///  process. The function initializes the memory it allocates to zero, unless MEM_RESET is 
      ///  used.
      /// </summary>
      /// <param name="hProcess">Handle to the process</param>
      /// <param name="lpAddress">
      ///  The pointer that specifies a desired starting address for the region of pages that you
      ///     want to allocate.
      ///  If you are reserving memory, the function rounds this address down to the nearest
      ///     multiple of the allocation granularity.
      ///  If you are committing memory that is already reserved, the function rounds this address
      ///     down to the nearest page boundary. To determine the size of a page and the allocation
      ///     granularity on the host computer, use the GetSystemInfo function.
      ///  If lpAddress is NULL, the function determines where to allocate the region.
      /// </param>
      /// <param name="dwSize">
      ///  The size of the region of memory to allocate, in bytes.
      ///  If lpAddress is NULL, the function rounds dwSize up to the next page boundary.
      ///  If lpAddress is not NULL, the function allocates all pages that contain one or more
      ///     bytes in the range from lpAddress to lpAddress+dwSize. This means, for example,
      ///     that a 2-byte range that straddles a page boundary causes the function to allocate both
      ///     pages
      /// </param>
      /// <param name="flAllocationType">The type of memory allocation. </param>
      /// <param name="flProtect">
      ///  The memory protection for the region of pages to be allocated. 
      ///  If the pages are being committed, you can specify any one of the
      /// </param>
      /// <returns></returns>
      [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
      public static extern IntPtr VirtualAllocEx(
         IntPtr hProcess,
         IntPtr lpAddress,
         uint dwSize,
         AllocationType flAllocationType,
         MemoryProtection flProtect
      );

      /// <summary>
      /// Releases, decommits, or releases and decommits a region of memory within the 
      /// virtual address space of a specified process.
      /// </summary>
      /// <param name="hProcess">Handle to the process whose VM we are freeing</param>
      /// <param name="lpAddress">Pointer to starting address of the target memory region</param>
      /// <param name="dwSize">Size of the memory region to free, in bytes</param>
      /// <param name="dwFreeType">The type of free operation.</param>
      /// <returns></returns>
      [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
      public static extern bool VirtualFreeEx(
         IntPtr hProcess,
         IntPtr lpAddress,
         int dwSize,
         FreeType dwFreeType
      );

      /// <summary>
      ///  HANDLE WINAPI CreateRemoteThread(
      ///     __in   HANDLE hProcess,
      ///     __in   LPSECURITY_ATTRIBUTES lpThreadAttributes,
      ///     __in   SIZE_T dwStackSize,
      ///     __in   LPTHREAD_START_ROUTINE lpStartAddress,
      ///     __in   LPVOID lpParameter,
      ///     __in   DWORD dwCreationFlags,
      ///     __out  LPDWORD lpThreadId
      ///  );
      /// </summary>
      /// <param name="hProcess">Handle to the given process</param>
      /// <param name="lpThreadAttributes">null</param>
      /// <param name="dwStackSize">0 for default</param>
      /// <param name="lpStartAddress">Pointer on other process for ThreadStart</param>
      /// <param name="lpParameter">
      ///  Parameter passed to the invoked method.  
      ///  Since we're calling LoadLibraryA, we pass the pointer to the library name.
      /// </param>
      /// <param name="dwCreationFlags">Zero so the thread starts immediately</param>
      /// <param name="lpThreadId">Out variable for the created thread's ID</param>
      [DllImport("kernel32")]
      public static extern IntPtr CreateRemoteThread(
         IntPtr hProcess,
         IntPtr lpThreadAttributes,
         uint dwStackSize,
         UIntPtr lpStartAddress,
         IntPtr lpParameter,
         uint dwCreationFlags,
         out uint lpThreadId
      );

      /// <summary>
      /// Gets a handle to the process of the given process id.
      /// This is necessary because you cannot use Process.Handle:
      ///  "Only processes started through a call to BeginAggregate set the 
      ///   Handle property of the corresponding Process instances."
      /// </summary>
      /// <param name="dwDesiredProcessAccess"></param>
      /// <param name="bInheritHandle">A BOOL - True if this caller process should inherit the handle, false otherwise</param>
      /// <param name="dwProcessId">PID of the process which we are opening a handle to</param>
      /// <returns></returns>
      [DllImport("kernel32.dll")]
      public static extern IntPtr OpenProcess(
         ProcessAccessRights dwDesiredProcessAccess,
         Int32 bInheritHandle,
         Int32 dwProcessId
      );

      /// <summary>
      /// Closes a handle
      /// </summary>
      /// <param name="hObject">The handle to close</param>
      /// <returns>successful</returns>
      [DllImport("kernel32.dll", SetLastError=true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool CloseHandle(
         IntPtr hObject
      );

      /// <summary>
      /// Gets the address of an exported procedure in the given module.
      /// </summary>
      /// <param name="hModule">Handle to a loaded module</param>
      /// <param name="procName">Name of exported procedure whose address we are finding</param>
      /// <returns>Pointer to the procedure or NULL</returns>
      [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
      public static extern UIntPtr GetProcAddress(
         IntPtr hModule,
         string procName
      );

      /// <summary>
      /// Writes data to an area of memory in a specified process. 
      /// The entire area to be written to must be accessible or the operation fails.
      /// </summary>
      /// <param name="hProcess">
      ///  A handle to the process memory to be modified. 
      /// The handle must have PROCESS_VM_WRITE and PROCESS_VM_OPERATION access to the process.
      /// </param>
      /// <param name="lpBaseAddress">
      ///  A pointer to the base address in the specified process to which data is written. 
      ///  Before data transfer occurs, the system verifies that all data in the base address 
      ///  and memory of the specified size is accessible for write access, and if it is not 
      ///  accessible, the function fails.
      /// </param>
      /// <param name="lpBuffer">
      ///  A pointer to the buffer that contains data to be written in the address space 
      ///  of the specified process.
      /// </param>
      /// <param name="nSize">The number of bytes to be written to the specified process.</param>
      /// <param name="lpNumberOfBytesWritten">
      ///  A pointer to a variable that receives the number of bytes transferred into the
      ///  specified process. This parameter is optional. If lpNumberOfBytesWritten is NULL, the
      ///  parameter is ignored.
      /// </param>
      /// <returns></returns>
      [DllImport("kernel32.dll", SetLastError = true)]
      public static extern bool WriteProcessMemory(
         IntPtr hProcess,
         IntPtr lpBaseAddress,
         byte[] lpBuffer,
         uint nSize,
         out int lpNumberOfBytesWritten //FIXME: not sure if this is right
      );

      /// <summary>
      /// Gets the handle of a loaded module
      /// </summary>
      /// <param name="lpModuleName">The name of the module (ex: Kernel32.dll)</param>
      /// <returns>Handle to the module or NULL</returns>
      [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr GetModuleHandle(
         string lpModuleName
      );

      /// <summary>
      /// In the case of a process or thread, this waits for the object to complete (ex: reach end of execution)
      /// </summary>
      /// <param name="handle"></param>
      /// <param name="milliseconds"></param>
      /// <returns></returns>
      [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
      public static extern WaitForSingleObjectResult WaitForSingleObject(
         IntPtr handle,
         Int32 milliseconds
      );
   }
}
