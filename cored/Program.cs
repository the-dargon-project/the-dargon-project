using System.Threading;
using NLog;

namespace Dargon.Daemon {
   public static class Program {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int kDaemonManagementPort = 21000;

      public static void Main(string[] args) {
         new CoreDaemonApplication().Start(null);
         new CountdownEvent(1).Wait();
      }
   }
}
