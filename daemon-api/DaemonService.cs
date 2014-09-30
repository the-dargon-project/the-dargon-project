using System;

namespace Dargon.Daemon
{
   public interface DaemonService
   {
      IDaemonConfiguration Configuration { get; }

      event EventHandler BeginShutdown;
      bool IsShutdownSignalled { get; }
      void Shutdown();
   }
}
