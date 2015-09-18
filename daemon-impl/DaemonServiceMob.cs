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
      public string GetConfiguration() => daemonService.Configuration.ToString();

      [ManagedOperation]
      public bool IsShutdownSignalled() => daemonService.IsShutdownSignalled;

      [ManagedOperation]
      public void Shutdown() => daemonService.Shutdown();
   }
}
