using Dargon.Game;

namespace Dargon.Modifications
{
   public interface ModificationImportService
   {
      IModification ImportLegacyModification(GameType gameType, string root, string[] filePaths);
   }
}
