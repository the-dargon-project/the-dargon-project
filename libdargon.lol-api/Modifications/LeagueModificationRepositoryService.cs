using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dargon.ModificationRepositories;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   [Guid("F737CEAA-B9FB-4FBB-9938-52743038B414")]
   public interface LeagueModificationRepositoryService : ModificationRepositoryService
   {
      IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths);
      IEnumerable<IModification> EnumerateModifications();
   }
}