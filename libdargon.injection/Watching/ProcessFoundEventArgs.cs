using System;

namespace Dargon.Processes.Watching
{
   /// <summary>
   /// 
   /// </summary>
   public class ProcessFoundEventArgs : EventArgs
   {
      /// <summary>
      /// Initializes a new instance of a ProcessFoundEventArgs, storing the given parameters into
      /// the properties of the event argument.
      /// </summary>
      /// <param name="processName">
      /// The name of the process
      /// </param>
      /// <param name="processId">
      /// The Process ID of the process
      /// </param>
      /// <param name="parentProcessId">
      /// The Process ID of the process's parent.
      /// </param>
      public ProcessFoundEventArgs(
         string processName,
         int processId,
         int parentProcessId)
      {
         ProcessName = processName;
         ProcessID = processId;
         ParentProcessID = parentProcessId;
      }

      //-------------------------------------------------------------------------------------------
      // Event Out
      //-------------------------------------------------------------------------------------------
      /// <summary>
      /// The name of the process, IE:
      /// LoLLauncher.exe, lol.launcher.exe, LolClient.exe
      /// </summary>
      public string ProcessName { get; private set; }

      /// <summary>
      /// The ProcessID of the process
      /// </summary>
      public int ProcessID { get; private set; } //Pretty sure this is only a ushort

      /// <summary>
      /// The ProcessID of the process's parent
      /// </summary>
      public int ParentProcessID { get; private set; } //Pretty sure this is only a ushort
   }
}