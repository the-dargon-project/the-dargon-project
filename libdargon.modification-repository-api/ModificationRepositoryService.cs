using Dargon.Game;
using Dargon.Modifications;
using System.Collections.Generic;

namespace Dargon.ModificationRepositories
{
   public interface ModificationRepositoryService
   {
      IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths, GameType gameType = null);
      void DeleteModification(IModification modification);
      IEnumerable<IModification> EnumerateModifications(GameType gameType = null);
   }
}
