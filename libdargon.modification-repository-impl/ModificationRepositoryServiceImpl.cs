using Dargon.Daemon;
using Dargon.Game;
using Dargon.Modifications;
using Dargon.Patcher;
using ItzWarty;
using ItzWarty.IO;
using LibGit2Sharp;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Dargon.ModificationRepositories
{
   public class ModificationRepositoryServiceImpl : ModificationRepositoryService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const string kRepositorySubdirectoryName = "repositories";
      private const int PATH_DELIMITER_LENGTH = 1;

      private readonly IClientConfiguration configuration;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IModificationLoader modificationLoader;
      private readonly IModificationMetadataSerializer modificationMetadataSerializer;
      private readonly IModificationMetadataFactory modificationMetadataFactory;
      private string repositorySubdirectoryPath;

      public ModificationRepositoryServiceImpl(IClientConfiguration configuration, IFileSystemProxy fileSystemProxy, IModificationLoader modificationLoader, IModificationMetadataSerializer modificationMetadataSerializer, IModificationMetadataFactory modificationMetadataFactory)
      {
         logger.Info("Constructing Modification Repository Service");

         this.configuration = configuration;
         this.fileSystemProxy = fileSystemProxy;
         this.modificationLoader = modificationLoader;
         this.modificationMetadataSerializer = modificationMetadataSerializer;
         this.modificationMetadataFactory = modificationMetadataFactory;
      }

      public void Initialize()
      {
         logger.Info("Initializing Modification Repository Service");

         repositorySubdirectoryPath = Path.Combine(configuration.UserDataDirectoryPath, kRepositorySubdirectoryName);
         fileSystemProxy.PrepareDirectory(repositorySubdirectoryPath);
      }

      public IModification GetModificationOrNull(string repositoryName) {
         try {
            var directoryInfo = fileSystemProxy.GetDirectoryInfo(Path.Combine(repositorySubdirectoryPath, repositoryName));
            if (!directoryInfo.Exists) {
               logger.Warn("Tried to get modification \"" + repositoryName + "\" but directory did not exist.");
               return null;
            }
            return modificationLoader.Load(directoryInfo.Name, directoryInfo.FullName);
         } catch (Exception e) {
            logger.Warn("Unable to load modification \"" + repositoryName + "\".");
            logger.Warn(e.ToString());
            return null;
         }
      }

      public void DeleteModification(IModification modification)
      {
         logger.Info("Removing Modification " + modification + " at " + modification.RepositoryPath);
         fileSystemProxy.DeleteDirectory(modification.RepositoryPath, true);
      }

      public IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths, GameType gameType)
      {
         gameType = gameType ?? GameType.Any;

         logger.Info("Importing Legacy Modification \"{0}\" from {1} for {2}".F(repositoryName, sourceRoot, gameType.Name));
         sourceRoot = Path.GetFullPath(sourceRoot);
         sourceFilePaths = Util.Generate(sourceFilePaths.Length, i => Path.GetFullPath(sourceFilePaths[i]));

         var repositoryPath = Path.Combine(repositorySubdirectoryPath, repositoryName);
         fileSystemProxy.PrepareDirectory(repositoryPath);

         Repository.Init(repositoryPath);
         var gitRepository = new Repository(repositoryPath);
         var dpmRepository = new LocalRepository(repositoryPath);
         using (dpmRepository.TakeLock()) {
            dpmRepository.Initialize();
            var metadata = modificationMetadataFactory.Create(repositoryName, gameType);
            foreach (var sourceFilePath in sourceFilePaths) {
               var internalPath = Path.Combine(metadata.ContentPath, sourceFilePath.Substring(sourceRoot.Length + PATH_DELIMITER_LENGTH));
               var absolutePath = dpmRepository.GetAbsolutePath(internalPath);
               fileSystemProxy.PrepareParentDirectory(absolutePath);
               fileSystemProxy.CopyFile(sourceFilePath, absolutePath);

               gitRepository.Stage(internalPath);
               dpmRepository.AddFile(internalPath);
            }
            var metadataFilePath = dpmRepository.GetAbsolutePath(ModificationConstants.kMetadataFileName);
            modificationMetadataSerializer.Save(metadataFilePath, metadata);
            gitRepository.Stage(metadataFilePath);
            gitRepository.Commit("Initial Commit");
         }
         return modificationLoader.Load(repositoryName, repositoryPath);
      }

      public IEnumerable<IModification> EnumerateModifications(GameType gameType)
      {
         foreach (var directory in fileSystemProxy.EnumerateDirectories(repositorySubdirectoryPath)) {
            logger.Info("Candidate modification " + directory );
            IModification modification = null;
            try {
               modification = modificationLoader.Load(fileSystemProxy.GetDirectoryInfo(directory).Name, directory);
            } catch (Exception e) {
               logger.Warn("Unable to load modification at path \"" + directory + "\".");
               logger.Warn(e.ToString());
            }
            if (modification != null && (gameType == null || gameType == GameType.Any || modification.Metadata.Targets.Contains(gameType))) {
               yield return modification;
            }
         }
      }
   }
}
