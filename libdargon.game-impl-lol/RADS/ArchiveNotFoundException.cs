using System;

namespace Dargon.LeagueOfLegends.RADS
{
   public class ArchiveNotFoundException : Exception
   {
      public ArchiveNotFoundException(uint version)
         : base("The given archive " + version + " was not found!")
      {

      }
   }
}