using Dargon.IO.RADS;

namespace Dargon.LeagueOfLegends.RADS
{
   public class RadsProjectReference : IRadsProjectReference
   {
      private RiotProject value;

      public RadsProjectReference(RiotProject value)
      {
         this.value = value;
      }

      public void Dispose() { value = null; }

      public RiotProject Value { get { return value; } }
   }
}