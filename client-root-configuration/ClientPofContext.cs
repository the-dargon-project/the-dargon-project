using Dargon.LeagueOfLegends;
using Dargon.Management;
using Dargon.Modifications;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.Processes;
using Dargon.Services.Messaging;
using Dargon.Trinkets;

namespace Dargon {
   public class ClientPofContext : PofContext {
      public ClientPofContext() {
         // Nest Exeggutor 3000-3999
         MergeContext(new ExeggutorPofContext(3000));
      }
   }
}