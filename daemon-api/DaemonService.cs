using System;
using System.Runtime.InteropServices;

namespace Dargon.Daemon {
   [Guid("01C78C0B-970F-43E7-B71A-4F6125CEBE1F")]
   public interface DaemonService
   {
      IDaemonConfiguration Configuration { get; }

      event EventHandler BeginShutdown;
      bool IsShutdownSignalled { get; }
      void Shutdown();
   }
}
