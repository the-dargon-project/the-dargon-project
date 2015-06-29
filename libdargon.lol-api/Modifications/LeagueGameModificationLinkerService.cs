using Dargon.Trinkets.Commands;
using System;
using System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Modifications {
   public interface LeagueGameModificationLinkerService
   {
      CommandList LinkModificationObjects();
   }

   public interface LeagueGameModificationLinkerResult
   {
      IReadOnlyDictionary<uint, Tuple<string, string>> ArchiveAndDataOverridesById { get; }
      string ReleaseManifestOverridePath { get; }
   }
}
