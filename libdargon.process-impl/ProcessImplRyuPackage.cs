using Dargon.Processes.Watching;
using Dargon.Ryu;

namespace Dargon.Processes {
   public class ProcessImplRyuPackage : RyuPackageV1 {
      public ProcessImplRyuPackage() {
         Singleton<IProcessDiscoveryMethodFactory, ProcessDiscoveryMethodFactory>();
         Singleton<IProcessDiscoveryMethod>(ryu => ryu.Get<IProcessDiscoveryMethodFactory>().CreateOptimalProcessDiscoveryMethod());
         Singleton<IProcessWatcher, ProcessWatcher>();
         Singleton<ProcessWatcherService, ProcessWatcherServiceImpl>();
         PofContext<ProcessImplPofContext>();
      }
   }
}
