using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Services;

namespace Dargon {
   public class DargonServiceConfiguration : ServiceConfiguration {
      private const int kDargonPort = 21337;
      private const int kHeartBeatInterval = 30000;

      public DargonServiceConfiguration() : base(kDargonPort, kHeartBeatInterval) {
      }
   }
}
