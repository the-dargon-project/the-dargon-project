using NLog;
using System;
using System.Threading;
using Dargon.Nest.Egg;

namespace Dargon.Daemon {
   public class DaemonServiceImpl : DaemonService {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IEggHost host;
      private readonly IClientConfiguration configuration;
      private bool isShutdownSignalled = false;

      public event EventHandler ShuttingDown;

      public DaemonServiceImpl(IEggHost host, IClientConfiguration configuration) {
         logger.Info("Initializing Daemon");

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