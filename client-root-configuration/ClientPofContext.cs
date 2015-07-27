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
         // Dargon Service Protocol 1-999
         MergeContext(new DspPofContext());

         // Dargon Management 1000-1999
         MergeContext(new ManagementPofContext());

         // Hydar 2000-2999

         // Nest Exeggutor 3000-3999
         MergeContext(new ExeggutorPofContext(3000));

         // Modification-Api and Impl 10000-10999
         MergeContext(new ModificationsApiPofContext());
         MergeContext(new ModificationsImplPofContext());

         // Trinket 11000-11999
         MergeContext(new TrinketsApiPofContext());
         MergeContext(new TrinketsImplPofContext());

         // League of Legends Api and Impl 12000-12999
         MergeContext(new LeagueOfLegendsApiPofContext());

         // Process-API and Impl 13000-13999
         MergeContext(new ProcessImplPofContext());

         // Wyvern Account-API 1000000-1000999
      }
   }
}