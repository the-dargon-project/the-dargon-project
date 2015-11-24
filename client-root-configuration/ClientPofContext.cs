using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;

namespace Dargon {
   public class ClientPofContext : PofContext {
      public ClientPofContext() {
         // Nest Exeggutor 3000-3999
         MergeContext(new ExeggutorPofContext(3000));
      }
   }
}