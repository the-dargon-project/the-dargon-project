using System;
using System.Management;

namespace Dargon.Processes.Watching
{
   public class WmiProcessDiscoveryMethod : IProcessDiscoveryMethod
   {
      /// <summary>
      /// This event is fired when a new victim process is found.
      /// </summary>
      public event OnProcessDiscovered ProcessDiscovered;

      /// <summary>
      /// Fires events whenever a process starts
      /// </summary>
      private ManagementEventWatcher m_processStartWatcher = null;

      /// <summary>
      /// Initializes a new instance of a WmiProcessDiscoveryMethod, which alerts us of when processes start
      /// by using WMI events.  This is better than the PollingProcessDiscoveryMethod counterpart.
      /// </summary>
      public WmiProcessDiscoveryMethod()
      {
         m_processStartWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
         m_processStartWatcher.EventArrived += ProcessStartHandler;
      }

      /// <summary>
      /// This event is fired whenever a new process starts.
      /// </summary>
      /// <param name="sender">
      /// Don't care, though it's probably the ManagementEventWatcher
      /// </param>
      /// <param name="e">
      /// Event Arguments which tell us info about the process which just started.
      /// </param>
      private void ProcessStartHandler(object sender, EventArrivedEventArgs e)
      {
         var eventArgs = new ProcessFoundEventArgs(
            e.NewEvent.Properties["ProcessName"].Value.ToString(),
            Int32.Parse(e.NewEvent.Properties["ProcessID"].Value.ToString()),
            Int32.Parse(e.NewEvent.Properties["ParentProcessID"].Value.ToString())
         );

         var capture = ProcessDiscovered;
         if (capture != null)
            capture("ProcessWatcher", eventArgs);
      }

      /// <summary>
      /// Starts stalking new processes
      /// </summary>
      public void Start()
      {
         m_processStartWatcher.Start();
      }

      /// <summary>
      /// Stops stalking new processes
      /// </summary>
      public void Stop()
      {
         m_processStartWatcher.Stop();
      }
   }
}
