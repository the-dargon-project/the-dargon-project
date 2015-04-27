using Dargon.RADS.Archives;

namespace Dargon.LeagueOfLegends.RADS {
   public class RadsArchiveReference : IRadsArchiveReference
   {
      private RiotArchive value;

      public RadsArchiveReference(RiotArchive value) {
         this.value = value;
      }

      public void Dispose() { value = null; }

      public RiotArchive Value { get { return value; } }
   }
}