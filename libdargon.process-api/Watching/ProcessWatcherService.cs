using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Processes.Watching
{
   public interface ProcessWatcherService
   {
      void Subscribe(Action<CreatedProcessDescriptor> handler, IEnumerable<string> names, bool retroactive = false);
   }
}
