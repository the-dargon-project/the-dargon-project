using Dargon.Game;

namespace Dargon.Modifications
{
   public interface ModificationImportService
   {
      IModification ImportLegacyModification(string root, string[] filePaths, GameType gameType);
   }
}
