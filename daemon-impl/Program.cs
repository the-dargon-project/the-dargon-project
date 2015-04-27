using NLog;

namespace Dargon.Daemon {
   public static class Program {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int kDaemonManagementPort = 21000;

      public static void Main(string[] args) {
         new CoreDaemonApplicationEgg().Start(null);
      }
   }
}
