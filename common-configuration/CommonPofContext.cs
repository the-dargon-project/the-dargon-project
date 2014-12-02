using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Modifications;

namespace Dargon {
   public class CommonPofContext : PofContext {
      public CommonPofContext() {
         // Dargon Service Protocol 1-1000
         MergeContext(new DspPofContext());

         // Modification-Impl 1001-2000
         MergeContext(new ModificationPofContext());
      }
   }
}