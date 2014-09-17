using System;

namespace Dargon.Processes.Kernel
{
   [Flags]
   public enum ProcessAccessRights : uint
   {
      PROCESS_ALL_ACCESS = SYNCHRONIZE | WRITE_OWNER | WRITE_DAC | READ_CONTROL | DELETE |
                           PROCESS_CREATE_PROCESS | PROCESS_CREATE_THREAD | PROCESS_DUP_HANDLE | PROCESS_QUERY_INFORMATION | PROCESS_QUERY_LIMITED_INFORMATION |
                           PROCESS_SET_INFORMATION | PROCESS_SET_QUOTA | PROCESS_SUSPEND_RESUME | PROCESS_TERMINATE | 
                           PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE,
      //-------------------------------------------------------------------------------------------
      // Standard Access Rights
      //-------------------------------------------------------------------------------------------
      /// <summary>
      /// The right to use the object for synchronization. 
      /// This enables a thread to wait until the object is in the signaled state.
      /// </summary>
      SYNCHRONIZE    = 0x00100000U,

      /// <summary>
      /// Required to change the owner in the security descriptor for the object.
      /// </summary>
      WRITE_OWNER    = 0x00080000U,
      
      /// <summary>
      /// Required to modify the DACL in the security descriptor for the object.
      /// </summary>
      WRITE_DAC      = 0x00040000U,

      /// <summary>
      /// Required to read information in the security descriptor for the object, 
      /// not including the information in the SACL. 
      /// To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. 
      /// </summary>
      READ_CONTROL   = 0x00020000U,

      /// <summary>
      /// Required to delete the object.
      /// </summary>
      DELETE         = 0x00010000U,

      ZUndefined1    = 0x00008000U,
      ZUndefined2    = 0x00004000U,
      ZUndefined3    = 0x00002000U,
      
      //-------------------------------------------------------------------------------------------
      // Process-Handle-Specific Stuff
      //-------------------------------------------------------------------------------------------
      /// <summary>
      /// Required to retrieve certain information about a process (see GetExitCodeProcess, GetPriorityClass, 
      /// IsProcessInJob, QueryFullProcessImageName). 
      /// A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted 
      /// PROCESS_QUERY_LIMITED_INFORMATION.
      /// </summary>
      PROCESS_QUERY_LIMITED_INFORMATION   = 0x00001000U,

      /// <summary>
      /// Required to suspend or resume a process.
      /// </summary>
      PROCESS_SUSPEND_RESUME              = 0x00000800U,

      /// <summary>
      /// Required to retrieve certain information about a process, such as its token, exit code, 
      /// and priority class (see OpenProcessToken).
      /// </summary>
      PROCESS_QUERY_INFORMATION           = 0x00000400U,

      /// <summary>
      /// Required to set certain information about a process, such as its priority class.
      /// </summary>
      PROCESS_SET_INFORMATION             = 0x00000200U,

      /// <summary>
      /// Required to set memory limits using SetProcessWorkingSetSize.
      /// </summary>
      PROCESS_SET_QUOTA                   = 0x00000100U,

      /// <summary>
      /// Required to create a process.
      /// </summary>
      PROCESS_CREATE_PROCESS              = 0x00000080U,

      /// <summary>
      /// Required to duplicate a handle using DuplicateHandle.
      /// </summary>
      PROCESS_DUP_HANDLE                  = 0x00000040U,

      /// <summary>
      /// Required to write to memory in a process using WriteProcessMemory.
      /// </summary>
      PROCESS_VM_WRITE                    = 0x00000020U,

      /// <summary>
      /// Required to read memory in a process using ReadProcessMemory.
      /// </summary>
      PROCESS_VM_READ                     = 0x00000010U,

      /// <summary>
      /// Required to perform an operation on the address space of a process 
      /// (see VirtualProtectEx and WriteProcessMemory).
      /// </summary>
      PROCESS_VM_OPERATION                = 0x00000008U,

      ZUndefined4                         = 0x00000004U,

      /// <summary>
      /// Required to create a thread.
      /// </summary>
      PROCESS_CREATE_THREAD               = 0x00000002U,

      /// <summary>
      /// Required to terminate a process using TerminateProcess.
      /// </summary>
      PROCESS_TERMINATE                   = 0x00000001U
   }
}
