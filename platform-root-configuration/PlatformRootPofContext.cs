using Dargon.Management;
using Dargon.PortableObjects;
using Dargon.Services.PortableObjects;
using Dargon.Wyvern.Accounts;

namespace Dargon {
   public class PlatformRootPofContext : PofContext {
      public PlatformRootPofContext() {
         // Dargon Service Protocol 1-999
         MergeContext(new DspPofContext());

         // Dargon Management 1000-1999
         MergeContext(new ManagementPofContext());

         // Desktop Stuff 10000 - 999999

         // Wyvern account-api 1000000-1000999
         MergeContext(new AccountApiPofContext());

         // Wyvern account-impl 1001000-1001999
         MergeContext(new AccountImplPofContext());
      }
   }
}
