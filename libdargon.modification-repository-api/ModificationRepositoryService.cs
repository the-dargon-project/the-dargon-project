
using System.Collections;
using System.Collections.Generic;
using Dargon.Game;
using Dargon.Modifications;

namespace Dargon.ModificationRepositories
{
   public interface ModificationRepositoryService
   {
      void ClearModifications();
      void AddModification(IModification modification);
      void RemoveModification(IModification modification);
      IEnumerable<IModification> EnumerateModifications(GameType gameType = GameType.Any);
   }
}
