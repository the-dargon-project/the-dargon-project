using System.Runtime.InteropServices;
using Dargon.Game;
using Dargon.Modifications;
using System.Collections.Generic;

namespace Dargon.ModificationRepositories {
   [Guid("744B3BD8-80E2-4821-ACC3-D8DE584CE45C")]
   public interface ModificationRepositoryService
   {
      IModification GetModificationOrNull(string repositoryName);
      void DeleteModification(IModification modification);
      IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths, GameType gameType = null);
      IEnumerable<IModification> EnumerateModifications(GameType gameType = null);
   }
}
