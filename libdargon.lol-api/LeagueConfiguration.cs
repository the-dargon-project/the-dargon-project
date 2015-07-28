using System.IO;

namespace Dargon.LeagueOfLegends {
   public class LeagueConfiguration
   {
      public string RadsPath
      {
         get
         {
            if (Directory.Exists(@"V:\Riot Games\League of Legends\RADS"))
               return @"V:\Riot Games\League of Legends\RADS";
            else if (Directory.Exists(@"T:\Games\LeagueOfLegends\RADS"))
               return @"T:\Games\LeagueOfLegends\RADS";
            else
               return @"C:\Riot Games\League of Legends\RADS";
         }
      }
   }
}