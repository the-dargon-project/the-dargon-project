using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;
using Dargon.Services;

namespace Dargon.Client {
   public class DargonClientRyuPackage : RyuPackageV1 {
      public DargonClientRyuPackage() {
         Singleton<TemporaryFileService>(ryu => ryu.Get<ServiceClient>().GetService<TemporaryFileService>());
      }
   }
}
