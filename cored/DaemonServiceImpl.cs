using NLog;
using System;
using System.Threading;
using Dargon.Nest.Eggs;

namespace Dargon.Daemon {
   public class DaemonServiceImpl : DaemonService {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly HatchlingHost host;
      private readonly IClientConfiguration configuration;
      private bool isShutdownSignalled = false;

      public event EventHandler ShuttingDown;

      public DaemonServiceImpl(HatchlingHost host, IClientConfiguration configuration) {
         logger.Info("Constructing Daemon Service");

         this.host = host;
         this.configuration = configuration;
      }

      public IClientConfiguration Configuration => configuration;
      public bool IsShutdownSignalled => isShutdownSignalled;

      public void Shutdown() {
         if (!isShutdownSignalled) {
            isShutdownSignalled = true;
            ShuttingDown?.Invoke(this, new EventArgs());
            host.Shutdown();
         }
      }
   }
}