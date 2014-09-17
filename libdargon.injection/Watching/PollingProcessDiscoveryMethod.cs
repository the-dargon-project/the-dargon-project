using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Dargon.Processes.Watching
{
   public class PollingProcessDiscoveryMethod : IProcessDiscoveryMethod
   {
      public event OnProcessDiscovered ProcessDiscovered;

      private Thread m_stalkerThread;

      /// <summary>
      /// Lock for handling the injectedids list
      /// </summary>
      private static object m_injectedIdsLock = new object();

      /// <summary>
      /// Initializes a new instance of a PollingProcessDiscoveryMethod, which finds new processes by
      /// constantly polling the process list.  This method is not as efficient as its event based
      /// counterpart, WmiProcessDiscoveryMethod.  However, it is provided just in case WmiProcessDiscoveryMethod does
      /// not work for a user.
      /// </summary>
      public PollingProcessDiscoveryMethod()
      {
         m_stalkerThread = new Thread(StalkerThreadStart) { IsBackground = true };
      }

      /// <summary>
      /// Begins stalking a victim process
      /// </summary>
      public void Start()
      {
         if (m_stalkerThread.ThreadState != System.Threading.ThreadState.Running)
            m_stalkerThread.Start();
      }

      /// <summary>
      /// Stops stalking a victim process... FOR NOW
      /// </summary>
      public void Stop()
      {
         if (m_stalkerThread.ThreadState == System.Threading.ThreadState.Running)
            m_stalkerThread.Abort();
      }

      /// <summary>
      /// Thread Entry Point for our ProcessWatcher Thread
      /// </summary>
      private static void StalkerThreadStart()
      {
         //The thread runs until it is aborted by Stop.
         //This method restarts when BeginAggregate is called.
         object previousMatchesLock = new object();

         //The IDs of running processes which we have injected to.
         List<int> injectedProcessIds = new List<int>();
         while (true)
         {
            //Console.WriteLine("Enumerate Processes");
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
               //Console.WriteLine(process.ProcessName);
               try
               {
                  //Console.WriteLine(process.Modules[0].FileName);
                  if (process.ProcessName.Equals(/*TargetProcessName*/"", StringComparison.InvariantCultureIgnoreCase))//process.Modules[0].FileName.EndsWith(TargetProcessName))
                  {
                     //Console.WriteLine("  Found a match");
                     int processId = process.Id;
                     lock (m_injectedIdsLock)
                     {
                        if (!injectedProcessIds.Contains(processId))
                        {
                           injectedProcessIds.Add(processId);
                           process.Exited += (s, e) => { lock (m_injectedIdsLock) injectedProcessIds.Remove(processId); };
                           //NewProcessFound(this, process);
                        }
                     }
                  }
               }
               catch
               {
                  //Native exceptions are caught here
                  //Such as Win32Exceptions thrown if we try to look into a 32-bit process
               }
            }
            Thread.Sleep(333);
         }
      }
   }
}
