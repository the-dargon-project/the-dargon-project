using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dargon.Processes.Watching
{
   public delegate void OnProcessDiscovered(object sender, ProcessFoundEventArgs eventArgs);

   /// <summary>
   /// The processor stalker class monitors when processes start, firing its NewProcessFound event
   /// when that happens.  The class also provides functionality to inject into those target
   /// processes.
   /// 
   /// We want to avoid opening process handles for processes which we don't care about, and we 
   /// especially don't want to leak process handles.  The workflow for ProcessWatcher is as 
   /// follows:
   /// 
   /// 1. ProcessWatcher is Initialized.  NewProcessFound event handlers added.
   /// 2. NewProcessFound event handlers get a VictimFoundContext with the following info:
   ///    ParentProcessID, ProcessID, ProcessName
   /// 3. Event handlers can then call ProcessWatcher.Inject(processId, dllPath).
   /// </summary>
   public class ProcessWatcher
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      public event OnProcessDiscovered NewProcessFound;

      private IProcessDiscoveryMethod discoveryMethod = null;

      public ProcessWatcher()
      {
         if (IntPtr.Size != 4)
            logger.Warn("Process Stalker: IntPtr Size != 4"); //Ensure we're running in 32-bit or WoW64

         try {
            discoveryMethod = new WmiProcessDiscoveryMethod();
         } catch (Exception e) {
            logger.Warn("Unable to init WMIStalkerMethod: " + e.ToString());
            logger.Warn("Fall back to poll method - dargon will be less memory/cpu efficient");
            discoveryMethod = new PollingProcessDiscoveryMethod();
         }

         //Bubble events from our stalker method to the event handlers of the process stalker
         discoveryMethod.ProcessDiscovered += (a, b) => {
            var capture = NewProcessFound;
            if (capture != null) {
               try {
                  capture(a, b);
               } catch (Exception e) {
                  logger.Error("ERROR: " + e.ToString());
                  throw;
               }
            }
         };
      }

      public void Start() { discoveryMethod.Start(); }

      public void Stop() { discoveryMethod.Stop(); }

      /// <summary>
      /// Attempts to find a process by enumerating all processes and filtering through them with 
      /// the given callback.
      /// </summary>
      /// <param name="callback">
      /// Takes in a process and returns true if the process should be returned in the filtered
      /// list of processes.
      /// </param>
      public List<Process> FindProcess(Func<Process, bool> callback)
      {
         List<Process> result = new List<Process>();
         foreach (var process in Process.GetProcesses()) {
            try {
               if (callback(process))
                  result.Add(process);
            } catch {
               //Native exceptions are caught here
               //Such as Win32Exceptions thrown if we try to look into a 32-bit process
            }
         }
         return result;
      }
   }
}
