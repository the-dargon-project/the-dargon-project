using NLog;
using System;
using System.Threading;

namespace Dargon.Daemon {
   public class DaemonServiceImpl : DaemonService {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IDargonConfiguration configuration;
      private readonly CountdownEvent shutdownSignal = new CountdownEvent(1);
      private bool isShutdownSignalled = false;

      public event EventHandler BeginShutdown;

      public DaemonServiceImpl(IDargonConfiguration configuration) {
         logger.Info("Initializing Daemon");

         this.configuration = configuration;
      }

      public IDargonConfiguration Configuration { get { return configuration; } }
      public bool IsShutdownSignalled { get { return isShutdownSignalled; } }

      public void Run() {
         shutdownSignal.Wait();
      }

      public void Shutdown() {
         if (!isShutdownSignalled) {
            isShutdownSignalled = true;
            var capture = BeginShutdown;
            if (capture != null)
               capture(this, new EventArgs());
            shutdownSignal.Signal();
         }
      }
   }
}