using System.Collections.Generic;
using Dargon.ModificationRepositories;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationRepositoryService : ModificationRepositoryService
   {
      IEnumerable<IModification> EnumerateModifications();
   }
}