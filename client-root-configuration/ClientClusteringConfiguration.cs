using System.Net;
using Dargon.Services;
using Dargon.Services.Clustering;

namespace Dargon {
   public class ClientClusteringConfiguration : ClusteringConfigurationImpl {
      private const int kNestClusterPort = 21999;

      public ClientClusteringConfiguration() : base(IPAddress.Loopback, kNestClusterPort, ClusteringRole.HostOrGuest) { }
   }
}
