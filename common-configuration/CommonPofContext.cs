using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;

namespace Dargon {
   public class CommonPofContext : PofContext {
      public CommonPofContext() {
         // Dargon Service Protocol 0-1000
         MergeContext(new DspPofContext());

      }
   }
}