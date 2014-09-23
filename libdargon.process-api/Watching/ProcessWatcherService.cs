using System;
using System.Collections.Generic;

namespace Dargon.Processes.Watching
{
   public interface ProcessWatcherService
   {
      void Subscribe(Action<CreatedProcessDescriptor> handler, IEnumerable<string> names, bool retroactive = false);
   }
}
