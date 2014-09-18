using System;
using System.IO;
using Dargon.Patcher;
using ItzWarty;
using ItzWarty.Services;
using NLog;

namespace Dargon.ModificationRepositories
{
   public class ModificationRepositoryServiceImpl : ModificationRepositoryService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private const int PATH_DELIMITER_LENGTH = 1;

      public ModificationRepositoryServiceImpl(IServiceLocator serviceLocator)
      {
         logger.Info("Initializing Modification Repository Service");

         serviceLocator.RegisterService(typeof(ModificationRepositoryService), this);
      }

      public void ClearModifications() { }

      public void ImportLegacyModification(string root, string[] filePaths)
      {
         root = Path.GetFullPath(root);
         filePaths = Util.Generate(filePaths.Length, i => Path.GetFullPath(filePaths[i]));

         var repo = new LocalRepository(root);
         repo.Initialize();

         foreach (var filePath in filePaths) {
            var internalPath = filePath.Substring(root.Length + PATH_DELIMITER_LENGTH);
            repo.AddFile(internalPath);
         }

         repo.Commit("Initial Commit");
      }
   }
}
