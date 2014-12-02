using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Modifications;
using Dargon.Processes;

namespace Dargon {
   public class CommonPofContext : PofContext {
      public CommonPofContext() {
         // Dargon Service Protocol 1-1000
         MergeContext(new DspPofContext());

         // Modification-Impl 1001-2000
         MergeContext(new ModificationPofContext());

         // Process-API and Impl 2001-3000
         MergeContext(new ProcessImplPofContext());
      }
   }
}