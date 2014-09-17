using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Dargon.Processes.Kernel;
using NLog;

namespace Dargon.Processes.Injection
{
   /// <summary>
   /// Handles injection of dynamically linked libraries to remote processes
   /// </summary>
   public class ProcessInjector
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      /// <summary>
      /// Tries [attempts] times to inject [dllPath] into the remote process.  If the injection
      /// fails, it waits [attemptInterval] milliseconds before trying again.
      /// </summary>
      /// <param name="processId">
      /// The processID of the process we want to create a remote thread in
      /// </param>
      /// <param name="dllPath">
      /// Path to the DLL which we are injecting into the process.
      /// </param>
      /// <param name="attempts">
      /// The number of times we will attempt to inject into the process before giving up.
      /// </param>
      /// <param name="attemptInterval">
      /// The interval we will wait between consecutive injections.
      /// </param>
      public bool TryInject(
         int processId,
         string dllPath,
         int attempts = 100,
         int attemptInterval = 200)
      {
         attempts = Math.Max(1, attempts);
         attemptInterval = Math.Max(1, attemptInterval);

         bool success = false;
         for (int i = 0; i < attempts && !success; i++)
         {
            try
            {
               InjectToVictim(processId, Path.GetFullPath(dllPath));
               success = true;
            }
            catch (Exception e)
            {
               logger.Error("Threw exception while injecting dll " + dllPath);
               logger.Error(e.ToString());
               Thread.Sleep(attemptInterval);
            }
         }
         return success;
      }

      /// <summary>
      /// Injects the given assembly into the given victim process
      /// (Creates a remote thread that runs in the virtual address space of the given process)
      /// 
      /// TODO: Clean up method with gotos for error handling
      /// </summary>
      /// <param name="processId">
      /// The processID of the process we want to create a remote thread in
      /// </param>
      /// <param name="dllPath">
      /// Path to the DLL which we are injecting into the process.
      /// </param>
      public void InjectToVictim(int processId, string dllPath)
      {
         dllPath = Path.GetFullPath(dllPath);

         //----------------------------------------------------------------------------------------
         // Open Victim Process Handle
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Open Process Handle");
         IntPtr hProcess = Kernel32.OpenProcess(
            ProcessAccessRights.PROCESS_ALL_ACCESS,
            1 /* We do want to own the handle */,
            processId
         );

         if (hProcess == IntPtr.Zero)
         {
            throw new Exception("Could not open handle to given process!\n" +
                                "OpenProcess returned NULL Handle!" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath +
                                "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }


         //----------------------------------------------------------------------------------------
         // Allocate memory for the DLL Path in the Virtual Memory of the victim process
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Allocate Memory in Process");
         uint szDllPath = (uint)dllPath.Length + 1U; //We account for the NUL cstring terminator

         //pDllPathBuffer: pointer to string buffer that contains the dll path
         IntPtr pDllPathBuffer = Kernel32.VirtualAllocEx(
            hProcess,
            (IntPtr)null,
            szDllPath,
            AllocationType.Commit,
            MemoryProtection.ExecuteReadWrite
         );

         if (pDllPathBuffer == IntPtr.Zero)
         {
            throw new Exception("Could not allocate memory inside target process! \n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }

         //----------------------------------------------------------------------------------------
         // Write the dll path to the allocated buffer, which is inside the VM of the process
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Write DLL Path in Process");
         int bytesWritten;
         bool writeSuccessful = Kernel32.WriteProcessMemory(
            hProcess,
            pDllPathBuffer,
            Encoding.ASCII.GetBytes(dllPath),
            szDllPath,
            out bytesWritten
         );
         if (!writeSuccessful)
         {
            throw new Exception("Call to WriteProcessMemory failed! \n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nBytes Written: " + bytesWritten +
                                "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }
         if (bytesWritten != szDllPath)
         {
            throw new Exception("WriteProcessMemory did not write expected byte count! \n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nBytes Written: " + bytesWritten +
                                "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }

         //----------------------------------------------------------------------------------------
         // Get Kernel32 Module Handle
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Get Kernel32 Handle");
         IntPtr hKernel32 = Kernel32.GetModuleHandle("kernel32.dll");
         if (hKernel32 == IntPtr.Zero)
         {
            throw new Exception("Unable to get Kernel32 Handle!\n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nBytes Written: " + bytesWritten +
                                "\nhKernel32: " + hKernel32 +
                                "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }

         //----------------------------------------------------------------------------------------
         // Get LoadLibraryA
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Get PFunctionLoadLibraryA");
         UIntPtr pLoadLibraryA = Kernel32.GetProcAddress(hKernel32, "LoadLibraryA");
         if (pLoadLibraryA == UIntPtr.Zero)
         {
            throw new Exception("Unable to get function pointer to LoadLibrarayA!\n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nBytes Written: " + bytesWritten +
                                "\nhKernel32: " + hKernel32 +
                                "\npLoadLibraryA: " + pLoadLibraryA +
                                "\nErrno: " + Marshal.GetLastWin32Error()
            );
         }

         //----------------------------------------------------------------------------------------
         // Create Remote Thread with ThreadStart LoadLibraryA and parameter pDllPathBuffer
         //    LoadLibraryA takes const char*, which is what pDllPathBuffer essentially is
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Create Remote Thread");
         uint remoteThreadId = 0;
         IntPtr hRemoteThread = IntPtr.Zero;
         for (int i = 0; i < 10 && hRemoteThread == IntPtr.Zero; i++)
         {
            try
            {
               hRemoteThread = Kernel32.CreateRemoteThread(
                  hProcess,
                  IntPtr.Zero, // Default Thread Attributes
                  0, // Default Stack Size        
                  pLoadLibraryA, // Entry Point
                  pDllPathBuffer, // Pointer to Parameter
                  0, // Default Creation Flags
                  out remoteThreadId // out ThreadID
                  );
            }
            catch (Win32Exception)
            {
               //Win32 Exception
               Console.Write("Win32 Exception when creating remote thread... " + ((i < 9) ? " Trying again" : ""));
               remoteThreadId = 0;
               hRemoteThread = IntPtr.Zero;
            }
         }
         if (hRemoteThread == IntPtr.Zero)
         {
            throw new Exception("Could Not Create Remote Thread!\n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nBytes Written: " + bytesWritten +
                                "\nhKernel32: " + hKernel32 +
                                "\npLoadLibraryA: " + pLoadLibraryA +
                                "\nhRemoteThread: " + hRemoteThread +
                                "\nremoteThreadId: " + remoteThreadId +
                                "\nErrno: " + Marshal.GetLastWin32Error()
               );
         }

         //----------------------------------------------------------------------------------------
         // We assume that LoadLibraryA occurs relatively quickly.  
         // We give 15 seconds for the operation to complete.  If the operation doesn't 
         // complete by then, something's pretty much definitely gone wrong.
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Wait for Remote Thread Completion");
         WaitForSingleObjectResult waitResult = Kernel32.WaitForSingleObject(hRemoteThread, 15000);
         if (waitResult != WaitForSingleObjectResult.WAIT_OBJECT_0)
         {
            string exceptionString = "Our LoadLibraryA thread did not signal completion after 15s!\n" +
                                     "\nProcess Id: " + processId +
                                     "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                     "\nBytes Written: " + bytesWritten +
                                     "\nhKernel32: " + hKernel32 +
                                     "\npLoadLibraryA: " + pLoadLibraryA +
                                     "\nhRemoteThread: " + hRemoteThread +
                                     "\nremoteThreadId: " + remoteThreadId +
                                     "\nwaitResult: " + ((uint)waitResult).ToString("x") +
                                     "\nErrno: " + Marshal.GetLastWin32Error();
            if (!Kernel32.CloseHandle(hRemoteThread))
            {
               exceptionString += "\nUnable to close remote thread handle!" +
                                  "\nErrno: " + Marshal.GetLastWin32Error();
            }
            throw new Exception(exceptionString);
         }

         //----------------------------------------------------------------------------------------
         // Free the string buffer and close the thread handle
         //----------------------------------------------------------------------------------------
         Console.WriteLine("Free Remote Virtual Memory");
         bool freeSuccessful = Kernel32.VirtualFreeEx(
            hProcess,
            pDllPathBuffer,
            0,
            FreeType.Release
         );
         int freeErrno = Marshal.GetLastWin32Error();
         bool threadCloseSuccessful = Kernel32.CloseHandle(hRemoteThread);

         if (!freeSuccessful || !threadCloseSuccessful)
         {
            throw new Exception("Issues cleaning up after process injection!\n" +
                                "\nProcess Id: " + processId +
                                "\nDll Path: " + dllPath + " (size " + szDllPath + ")" +
                                "\nBytes Written: " + bytesWritten +
                                "\nhKernel32: " + hKernel32 +
                                "\npLoadLibraryA: " + pLoadLibraryA +
                                "\nhRemoteThread: " + hRemoteThread +
                                "\nremoteThreadId: " + remoteThreadId +
                                "\nVirtualFreeEx Errno: " + freeErrno +
                                "\nThread CloseHandle Errno: " + Marshal.GetLastWin32Error()
            );
         }
      }
   }
}
