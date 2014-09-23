using Dargon.Game;
using Dargon.Patcher;
using ItzWarty;
using ItzWarty.Services;
using NLog;
using System.IO;

namespace Dargon.Modifications
{
   public class ModificationImportServiceImpl : ModificationImportService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int PATH_DELIMITER_LENGTH = 1;

      public ModificationImportServiceImpl(ServiceLocator serviceLocator) { serviceLocator.RegisterService(typeof(ModificationImportService), this); }

      public IModification ImportLegacyModification(string root, string[] filePaths, GameType gameType)
      {
         root = Path.GetFullPath(root);
         filePaths = Util.Generate(filePaths.Length, i => Path.GetFullPath(filePaths[i]));

         var repo = new LocalRepository(root);
         repo.Initialize();

         foreach (var filePath in filePaths)
         {
            var internalPath = filePath.Substring(root.Length + PATH_DELIMITER_LENGTH);
            repo.AddFile(internalPath);
         }

         repo.Commit("Initial Commit");
         return new Modification(root, gameType);
      }
   }
}
