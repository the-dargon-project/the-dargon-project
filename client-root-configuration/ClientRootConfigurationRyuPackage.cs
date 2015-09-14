using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Nest.Eggxecutor;
using Dargon.Ryu;
using Dargon.Services;
using Dargon.Services.Clustering;

namespace Dargon {
   public class ClientRootConfigurationRyuPackage : RyuPackageV1 {
      public ClientRootConfigurationRyuPackage() {
         Singleton<ClusteringConfiguration, ClientClusteringConfiguration>();
         PofContext<ClientPofContext>();

         Singleton<ExeggutorService>(ryu => ryu.Get<ServiceClient>().GetService<ExeggutorService>());
      }
   }
}
