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

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationResolutionServiceImpl : LeagueModificationOperationServiceBase<IResolutionTask, ResolutionTask, LeagueModificationResolutionServiceImpl.ResolutionContext>, LeagueModificationResolutionService
   {
      private const string RESOLUTION_METADATA_FILE_NAME = "RESOLUTION";

      private readonly RadsService radsService;

      public LeagueModificationResolutionServiceImpl(DaemonService daemonService, RadsService radsService) 
         : base(daemonService) { this.radsService = radsService; }

      public IResolutionTask ResolveModification(IModification modification, ModificationTargetType target)
      {
         if (modification.GameType != GameType.LeagueOfLegends) {
            throw new InvalidOperationException("League Modification Resolution Service can only resolve League of Legends modifications!");
         }

         logger.Info("Resolving Modification " + modification + " for Target Type " + target);


         var newTask = new ResolutionTask(modification);
         AddTask(modification.LocalGuid, newTask, target);
         return newTask;
      }

      protected override ResolutionContext CreateContext(ResolutionTask task, ModificationTargetType target) { 
         var clientProject = target.HasFlag(ModificationTargetType.Client) ? radsService.GetProjectUnsafe(RiotProjectType.AirClient) : null;
         var gameProject = target.HasFlag(ModificationTargetType.Game) ? radsService.GetProjectUnsafe(RiotProjectType.GameClient) : null;
         return new ResolutionContext(task, target, clientProject, gameProject);
      }

      protected override void ProcessTaskContext(ResolutionContext context)
      {
         var task = context.Task;
         var modification = task.Modification;

         var clientResolver = context.Targets.HasFlag(ModificationTargetType.Client) ? new Resolver(context.ClientProject.ReleaseManifest.Root) : null;
         var gameResolver = context.Targets.HasFlag(ModificationTargetType.Game) ? new Resolver(context.GameProject.ReleaseManifest.Root) : null;

         var repository = new LocalRepository(modification.RootPath);
         using (repository.TakeLock()) {
            string resolutionMetadataFilepath = repository.GetMetadataFilePath(RESOLUTION_METADATA_FILE_NAME);
            using (var resolutionMetadata = new ModificationResolutionTable(resolutionMetadataFilepath)) {
               foreach (var indexEntry in repository.EnumerateIndexEntries()) {
                  if (indexEntry.Value.Flags.HasFlag(IndexEntryFlags.Directory)) {
                     continue;
                  }

                  string internalPath = indexEntry.Key;
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
                        resolutionEntry.FileRevision = indexEntry.Value.RevisionHash;
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
      }

      public class ResolutionContext : ITaskContext<ResolutionTask>
      {
         private readonly ResolutionTask task;
         private readonly ModificationTargetType targets;
         private readonly RiotProject clientProject;
         private readonly RiotProject gameProject;

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
      }
   }
}