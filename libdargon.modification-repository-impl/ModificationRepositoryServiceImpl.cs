using System;
using System.IO;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.Modifications;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.Services;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.ModificationRepositories
{
   public class ModificationRepositoryServiceImpl : ModificationRepositoryService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const string kRepositorySubdirectoryName = "repositories";

      private readonly IDaemonConfiguration configuration;
      private readonly IServiceLocator serviceLocator;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IModificationLoader modificationLoader;
      private string repositorySubdirectoryPath;

      public ModificationRepositoryServiceImpl(IDaemonConfiguration configuration, IServiceLocator serviceLocator, IFileSystemProxy fileSystemProxy, IModificationLoader modificationLoader)
      {
         logger.Info("Constructing Modification Repository Service");

         this.configuration = configuration;
         this.serviceLocator = serviceLocator;
         this.fileSystemProxy = fileSystemProxy;
         this.modificationLoader = modificationLoader;
      }

      public void Initialize()
      {
         logger.Info("Initializing Modification Repository Service");

         serviceLocator.RegisterService(typeof(ModificationRepositoryService), this);

         repositorySubdirectoryPath = Path.Combine(configuration.UserDataDirectoryPath, kRepositorySubdirectoryName);
         fileSystemProxy.PrepareDirectory(repositorySubdirectoryPath);
      }

      public void AddModification(IModification modification)
      {
         logger.Info("Adding Modification " + modification); 
//         modifications.TryAdd(modification); 
      }

      public void RemoveModification(IModification modification)
      {
         logger.Info("Removing Modification " + modification);
//         modifications.TryRemove(modification);
      }

      public IEnumerable<IModification> EnumerateModifications(GameType gameType)
      {
         foreach (var directory in fileSystemProxy.EnumerateDirectories(repositorySubdirectoryPath)) {
            IModification modification = null;
            try {
               modification = modificationLoader.Load(fileSystemProxy.GetDirectoryInfo(directory).Name, directory);
            } catch (Exception e) {
               logger.Warn("Unable to load modification at path \"" + directory + "\".");
               logger.Warn(e.ToString());
            }
            if (modification != null && (gameType == GameType.Any || modification.Metadata.Targets.Contains(gameType))) {
               yield return modification;
            }
         }
      }
   }
}
