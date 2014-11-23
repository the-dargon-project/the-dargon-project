using System;
using System.Collections.Generic;
using System.Diagnostics;
using ItzWarty.Processes;

namespace Dargon.Processes.Watching
{
   public interface IProcessWatcher
   {
      event OnProcessDiscovered NewProcessFound;
      void Start();
      void Stop();

      /// <summary>
      /// Attempts to find a process by enumerating all processes and filtering through them with 
      /// the given callback.
      /// </summary>
      /// <param name="callback">
      /// Takes in a process and returns true if the process should be returned in the filtered
      /// list of processes.
      /// </param>
      List<IProcess> FindProcess(Func<IProcess, bool> callback);
   }
}