using Dargon.Management;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Modifications;
using Dargon.Processes;

namespace Dargon {
   public class ClientPofContext : PofContext {
      public ClientPofContext() {
         // Dargon Service Protocol 1-999
         MergeContext(new DspPofContext());

         // Dargon Management 1000-1999
         MergeContext(new ManagementPofContext());

         // Hydar 2000-2999
         
         // Modification-Impl 10000-10999
         MergeContext(new ModificationPofContext());

         // Process-API and Impl 11000-11999
         MergeContext(new ProcessImplPofContext());

         // Wyvern Account-API 1000000-1000999
      }
   }
}