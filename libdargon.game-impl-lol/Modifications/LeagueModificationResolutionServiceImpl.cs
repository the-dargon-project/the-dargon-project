using Dargon.Daemon;
using Dargon.Game;
using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.Resolution;
using Dargon.LeagueOfLegends.RADS;
using Dargon.Modifications;
using Dargon.Patcher;
using System;
using System.Linq;
using ItzWarty;
using ItzWarty.Threading;
using LibGit2Sharp;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationResolutionServiceImpl : LeagueModificationOperationServiceBase<IResolutionTask, ResolutionTask, LeagueModificationResolutionServiceImpl.ResolutionContext>, LeagueModificationResolutionService
   {
      public const string RESOLUTION_METADATA_FILE_NAME = "RESOLUTION";

      private readonly RadsService radsService;

      public LeagueModificationResolutionServiceImpl(IThreadingProxy threadingProxy, DaemonService daemonService, RadsService radsService)
         : base(threadingProxy, daemonService) { this.radsService = radsService; }

      public IResolutionTask StartModificationResolution(IModification modification, ModificationTargetType target)
      {
         if (!modification.Metadata.Targets.Contains(GameType.LeagueOfLegends)) {
            throw new InvalidOperationException("League Modification Resolution Service can only resolve League of Legends modifications!");
         }

         logger.Info("Resolving Modification " + modification + " for Target Type " + target);


         var newTask = new ResolutionTask(modification);
         AddTask(modification.RepositoryName, newTask, target);
         return newTask;
      }

      protected override ResolutionContext CreateContext(ResolutionTask task, ModificationTargetType target) { 
         var clientProject = target.HasFlag(ModificationTargetType.Client) ? radsService.GetProjectUnsafe(RiotProjectType.AirClient) : null;
         var gameProject = target.HasFlag(ModificationTargetType.Game) ? radsService.GetProjectUnsafe(RiotProjectType.GameClient) : null;
         return new ResolutionContext(task, target, clientProject, gameProject);
      }

      protected override void ProcessTaskContext(ResolutionContext context)
      {
         try {
            var task = context.Task;
            var modification = task.Modification;
            var modificationMetadata = modification.Metadata;
            var contentPath = modificationMetadata.ContentPath.Trim('/', '\\');

            logger.Info("Resolving files of modification {0}".F(modificationMetadata.Name));
            logger.Info("  Content Path: {0}".F(contentPath));

            var clientResolver = context.Targets.HasFlag(ModificationTargetType.Client) ? new Resolver(context.ClientProject.ReleaseManifest.Root) : null;
            var gameResolver = context.Targets.HasFlag(ModificationTargetType.Game) ? new Resolver(context.GameProject.ReleaseManifest.Root) : null;

            var gitRepository = new Repository(modification.RepositoryPath);
            var dpmRepository = new LocalRepository(modification.RepositoryPath);
            using (dpmRepository.TakeLock()) {
               string resolutionMetadataFilepath = dpmRepository.GetMetadataFilePath(RESOLUTION_METADATA_FILE_NAME);
               using (var resolutionMetadata = new ModificationResolutionTable(resolutionMetadataFilepath)) {
                  foreach (var file in gitRepository.Index) {
                     if (!file.Path.StartsWith(contentPath, StringComparison.OrdinalIgnoreCase)) {
                        logger.Info("Ignoring \"" + file.Path + "\" as it is not in the content directory.");
                        continue;
                     }

                     string internalPath = file.Path;
                     Hash160 fileHash = Hash160.Parse(file.Id.Sha);
                     logger.Info("HAVE TO RESOLVE " + internalPath);
                     var resolutionEntry = resolutionMetadata.GetValueOrNull(internalPath);

                     if (resolutionEntry != null && (resolutionEntry.Target != ModificationTargetType.Invalid && !context.Targets.HasFlag(resolutionEntry.Target))) {
                        continue; // skip over prior successfully resolved entries
                     }

                     if (resolutionEntry == null) {
                        resolutionEntry = new ModificationResolutionTable.ResolutionMetadataValue(null, Hash160.Zero, ModificationTargetType.Invalid);
                        resolutionMetadata[internalPath] = resolutionEntry;
                     }

                     Tuple<ModificationTargetType, Resolver>[] resolvers;
                     if (context.Targets == ModificationTargetType.Game) {
                        resolvers = new[] { new Tuple<ModificationTargetType, Resolver>(ModificationTargetType.Game, gameResolver) };
                     } else if (context.Targets == ModificationTargetType.Client) {
                        resolvers = new[] { new Tuple<ModificationTargetType, Resolver>(ModificationTargetType.Client, clientResolver) };
                     } else if (context.Targets == (ModificationTargetType.Client | ModificationTargetType.Game)) {
                        if (resolutionEntry.Target == ModificationTargetType.Game) {
                           resolvers = new[] { new Tuple<ModificationTargetType, Resolver>(ModificationTargetType.Game, gameResolver), new Tuple<ModificationTargetType, Resolver>(ModificationTargetType.Client, clientResolver) };
                        } else {
                           resolvers = new[] { new Tuple<ModificationTargetType, Resolver>(ModificationTargetType.Client, clientResolver), new Tuple<ModificationTargetType, Resolver>(ModificationTargetType.Game, gameResolver) };
                        }
                     } else {
                        logger.Error("Unknown resolver target " + context.Targets);
                        continue;
                     }

                     bool resolutionSucceeded = false;
                     for (var i = 0; i < resolvers.Length && !resolutionSucceeded; i++) {
                        var resolver = resolvers[i];
                        var resolution = resolver.Item2.Resolve(internalPath, resolutionEntry.ResolvedPath).FirstOrDefault();
                        if (resolution != null) {
                           resolutionEntry.Target = resolver.Item1;
                           resolutionEntry.ResolvedPath = resolution.GetPath();
                           resolutionEntry.FileRevision = fileHash;
                           resolutionSucceeded = true;
                        }
                     }

                     if (resolutionSucceeded) {
                        logger.Info("Successfully resolved " + internalPath + " to " + resolutionEntry.ResolvedPath);
                     } else {
                        logger.Info("Failed to resolve " + internalPath);
                     }
                  }
               }
            }
         } finally {
            context.Dispose();
         }
      }

      public class ResolutionContext : ITaskContext<ResolutionTask>, IDisposable
      {
         private readonly ResolutionTask task;
         private readonly ModificationTargetType targets;
         private RiotProject clientProject;
         private RiotProject gameProject;

         public ResolutionContext(ResolutionTask task, ModificationTargetType targets, RiotProject clientProject, RiotProject gameProject)
         {
            this.task = task;
            this.targets = targets;
            this.clientProject = clientProject;
            this.gameProject = gameProject;
         }

         public ResolutionTask Task { get { return task; } }
         public ModificationTargetType Targets { get { return targets; } }
         public RiotProject ClientProject { get { return clientProject; } }
         public RiotProject GameProject { get { return gameProject; } }

         public void Dispose()
         {
            clientProject = null;
            gameProject = null;
         }
      }
   }
}