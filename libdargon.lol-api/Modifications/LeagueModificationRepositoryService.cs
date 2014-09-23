using System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Modifications
{
   public interface LeagueModificationRepositoryService
   {
      void ClearModifications();
      void AddModification(ILeagueModification modification);
      void RemoveModification(ILeagueModification modification);
      IEnumerable<ILeagueModification> EnumerateModifications();
   }
}