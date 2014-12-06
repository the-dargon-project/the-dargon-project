using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Management;

namespace Dargon.Daemon {
   public class DaemonServiceMob {
      private readonly DaemonServiceImpl daemonService;

      public DaemonServiceMob(DaemonServiceImpl daemonService) {
         this.daemonService = daemonService;
      }

      [ManagedOperation]
      public string GetConfiguration() {
         return daemonService.Configuration.ToString();
      }

      [ManagedOperation]
      public bool IsShutdownSignalled() {
         return daemonService.IsShutdownSignalled;
      }

      [ManagedOperation]
      public bool Shutdown() {
         daemonService.Shutdown();
         return true;
      }
   }
}
