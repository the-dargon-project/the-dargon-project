using System;
using System.IO;
using System.Linq;
using Dargon.Game;
using Dargon.InjectedModule.Tasks;
using Dargon.Modifications;
using Dargon.Patcher;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationTasklistCompilerServiceImpl : LeagueModificationTasklistCompilerService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly ITaskFactory taskFactory;

      public LeagueModificationTasklistCompilerServiceImpl(ITaskFactory taskFactory) { this.taskFactory = taskFactory; }

      public ITasklist BuildTasklist(IModification modification, ModificationTargetType target)
      {
         if (!modification.Metadata.Targets.Contains(GameType.LeagueOfLegends)) {
            throw new InvalidOperationException("League Modification Compilation Service can only build tasklist for League of Legends modifications!");
         }

         logger.Info("Building Tasklist for Modification " + modification + " for Target Type " + target);

         var result = new Tasklist();
         var repository = new LocalRepository(modification.RepositoryPath);
         using (repository.TakeLock()) {
            string compilationMetadataFilepath = repository.GetMetadataFilePath(LeagueModificationObjectCompilerServiceImpl.COMPILATION_METADATA_FILE_NAME); // HACK
            string resolutionMetadataFilepath = repository.GetMetadataFilePath(LeagueModificationResolutionServiceImpl.RESOLUTION_METADATA_FILE_NAME); // HACK
            using (var compilationMetadata = new ModificationCompilationTable(compilationMetadataFilepath))
            using (var resolutionTable = new ModificationResolutionTable(resolutionMetadataFilepath)) {
               foreach (var indexEntry in repository.EnumerateIndexEntries()) {
                  if (indexEntry.Value.Flags.HasFlag(IndexEntryFlags.Directory)) {
                     continue;
                  }

                  var internalPath = indexEntry.Key;
                  var resolutionValue = resolutionTable.GetValueOrNull(internalPath);
                  var compilationValue = compilationMetadata.GetValueOrNull(internalPath);

                  if (resolutionValue == null || compilationValue == null)
                     continue;
                  if (resolutionValue.Target == ModificationTargetType.Invalid || (resolutionValue.Target & target) == 0)
                     continue;

                  if (resolutionValue.Target.HasFlag(ModificationTargetType.Client)) {
                     result.Add(taskFactory.CreateFileSwapTask(resolutionValue.ResolvedPath, repository.GetAbsolutePath(internalPath)));
                  }
               }
            }
         }
         return result;
      }
   }
}