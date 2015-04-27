using Dargon.Services;

namespace Dargon {
   public class ClientClusteringConfiguration : ClusteringConfiguration {
      private const int kDargonPort = 21337;
      private const int kHeartBeatInterval = 30000;

      public ClientClusteringConfiguration() : base(kDargonPort, kHeartBeatInterval) {
      }
   }
}
