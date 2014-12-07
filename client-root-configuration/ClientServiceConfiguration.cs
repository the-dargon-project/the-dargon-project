using Dargon.Services;

namespace Dargon {
   public class ClientServiceConfiguration : ServiceConfiguration {
      private const int kDargonPort = 21337;
      private const int kHeartBeatInterval = 30000;

      public ClientServiceConfiguration() : base(kDargonPort, kHeartBeatInterval) {
      }
   }
}
