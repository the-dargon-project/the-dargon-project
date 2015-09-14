using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon.Daemon {
   public class DaemonImplRyuPackage : RyuPackageV1 {
      public DaemonImplRyuPackage() {
         Singleton<DaemonServiceImpl>();
         LocalService<DaemonService, DaemonServiceImpl>();
         Mob<DaemonServiceMob>();
      }
   }
}
