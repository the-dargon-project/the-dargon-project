using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Services;
using ParentProcessUtilities = Dargon.Processes.Kernel.ParentProcessUtilities;

namespace Dargon.Processes.Watching
{
   public class ProcessWatcherServiceImpl : ProcessWatcherService
   {
      private readonly ProcessWatcher processWatcher;
      private readonly MultiValueDictionary<string, Action<CreatedProcessDescriptor>> processSpawnedHandlersByProcessName = new MultiValueDictionary<string, Action<CreatedProcessDescriptor>>();

      public ProcessWatcherServiceImpl(IServiceLocator serviceLocator)
      {
         serviceLocator.RegisterService(typeof(ProcessWatcherService), this);
         
         processWatcher = new ProcessWatcher();
         processWatcher.NewProcessFound += HandleProcessWatcherNewProcessFound;
      }

      private void HandleProcessWatcherNewProcessFound(object sender, ProcessFoundEventArgs e)
      {
         var lowerProcessName = e.ProcessName;
         var handlers = processSpawnedHandlersByProcessName.GetValueOrDefault(lowerProcessName);
         if (handlers != null) {
            foreach (var handler in handlers) {
               handler(new CreatedProcessDescriptor(e.ProcessName, e.ProcessID, e.ParentProcessID));
            }
         }
      }

      public void Subscribe(Action<CreatedProcessDescriptor> handler, IEnumerable<string> names, bool retroactive)
      {
         var lowerCaseNames = new HashSet<string>(names.Select(FormatProcessName));
         foreach (var lowerCaseName in lowerCaseNames) {
            processSpawnedHandlersByProcessName.Add(lowerCaseName, handler);
         }

         if (retroactive) {
            var processes = processWatcher.FindProcess((p) => lowerCaseNames.Contains(FormatProcessName(p.ProcessName)));
            foreach (var process in processes) {
               handler(new CreatedProcessDescriptor(process.ProcessName, process.Id, ParentProcessUtilities.GetParentProcess(process.Id).Id));
            }
         }
      }

      private string FormatProcessName(string name) { return name.ToLowerInvariant(); }
   }
}
