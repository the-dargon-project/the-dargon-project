using System;

namespace Dargon.Daemon
{
   public interface DaemonService
   {
      event EventHandler BeginShutdown;
      bool IsShutdownSignalled { get; }
      void Shutdown();
   }
}
