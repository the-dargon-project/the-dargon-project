using System.Collections.Generic;
using Dargon.ModificationRepositories;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationRepositoryService : ModificationRepositoryService
   {
      IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths);
      IEnumerable<IModification> EnumerateModifications();
   }
}