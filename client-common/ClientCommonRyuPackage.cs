using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon {
   public class ClientCommonRyuPackage : RyuPackageV1 {
      public ClientCommonRyuPackage() {
         Singleton<IClientConfiguration, ClientConfiguration>();
         Singleton<ClientSystemStateFactory>();
         Singleton<SystemState>(ryu => ryu.Get<ClientSystemStateFactory>().Create());
         Mob<ClientSystemStateMob>();
      }
   }
}
