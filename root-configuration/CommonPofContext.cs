using Dargon.Management;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Modifications;
using Dargon.Processes;

namespace Dargon {
   public class CommonPofContext : PofContext {
      public CommonPofContext() {
         // Dargon Service Protocol 1-999
         MergeContext(new DspPofContext());

         // Dargon Management 1000-1999
         MergeContext(new ManagementPofContext());

         // Modification-Impl 10000-10999
         MergeContext(new ModificationPofContext());

         // Process-API and Impl 11000-11999
         MergeContext(new ProcessImplPofContext());
      }
   }
}