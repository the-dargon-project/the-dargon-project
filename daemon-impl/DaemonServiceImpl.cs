using ItzWarty.Services;
using NLog;
using System;
using System.Threading;

namespace Dargon.Daemon
{
   public class DaemonServiceImpl : DaemonService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IDaemonConfiguration configuration;
      private readonly CountdownEvent shutdownSignal = new CountdownEvent(1);
      private bool isShutdownSignalled = false;

      public event EventHandler BeginShutdown;

      public DaemonServiceImpl(IServiceLocator serviceLocator, IDaemonConfiguration configuration)
      {
         logger.Info("Initializing Daemon");
         serviceLocator.RegisterService(typeof(DaemonService), this);

         this.configuration = configuration;
      }

      public IDaemonConfiguration Configuration { get { return configuration; } }
      public bool IsShutdownSignalled { get { return isShutdownSignalled; } }

      public void Run() { shutdownSignal.Wait(); }

      public void Shutdown()
      {
         isShutdownSignalled = true;
         var capture = BeginShutdown;
         if (capture != null)
            capture(this, new EventArgs());
         shutdownSignal.Signal();
      }
   }
}