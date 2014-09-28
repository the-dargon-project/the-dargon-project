using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.Resolution;
using Dargon.LeagueOfLegends.RADS;
using Dargon.Modifications;
using Dargon.Patcher;
using ItzWarty;
using NLog;
using NLog.Targets;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationResolutionServiceImpl : LeagueModificationResolutionService
   {
      private const string RESOLUTION_METADATA_FILE_NAME = "RESOLUTION";
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly Dictionary<Guid, ResolutionTask> resolutionTasksByModificationLocalGuid = new Dictionary<Guid, ResolutionTask>();
      private readonly BlockingCollection<ResolutionContext> taskContextQueue = new BlockingCollection<ResolutionContext>(new ConcurrentQueue<ResolutionContext>());
      private readonly CancellationTokenSource shutdownCancellationTokenSource = new CancellationTokenSource();
      private readonly DaemonService daemonService;
      private readonly RadsService radsService;
      private readonly CancellationToken shutdownCancellationToken;
      private readonly Thread[] consumerThreads;

      public LeagueModificationResolutionServiceImpl(DaemonService daemonService, RadsService radsService)
      {
         this.daemonService = daemonService;
         this.radsService = radsService;

         this.shutdownCancellationToken = shutdownCancellationTokenSource.Token;

         this.consumerThreads = Util.Generate(
            Math.Max(1, Environment.ProcessorCount / 2),
            i => new Thread(() => ConsumerThreadStart(i)).With(t => t.Start())
         );
      }

      public IResolutionTask ResolveModification(IModification modification, ModificationTargetType type)
      {
         if (modification.GameType != GameType.LeagueOfLegends) {
            throw new InvalidOperationException("League Modification Resolution Service can only resolve League of Legends modifications!");
         }

         logger.Info("Resolving Modification " + modification + " for Target Type " + type);

         var clientProject = type.HasFlag(ModificationTargetType.Client) ? radsService.GetProjectUnsafe(RiotProjectType.AirClient) : null;
         var gameProject = type.HasFlag(ModificationTargetType.Game) ? radsService.GetProjectUnsafe(RiotProjectType.GameClient) : null;

         var newTask = new ResolutionTask(modification);

         ResolutionTask previousTask;
         if (resolutionTasksByModificationLocalGuid.TryGetValue(modification.LocalGuid, out previousTask)) {
            previousTask.SetNext(newTask);
            previousTask.Cancel();
         }

         taskContextQueue.Add(new ResolutionContext(newTask, type, clientProject, gameProject));
         return newTask;
      }

      private void ConsumerThreadStart(int id)
      {
         var prefix = "t" + id + ": ";
         logger.Info(prefix + "at entry point");
         while (!shutdownCancellationToken.IsCancellationRequested) {
            try
            {
               logger.Info(prefix + "taking task context...");
               var context = taskContextQueue.Take(shutdownCancellationToken);
               var task = context.Task;
               var modification = task.Modification;
               logger.Info(prefix + "got task " + task + " for modification " + modification);

               var clientResolver = context.Targets.HasFlag(ModificationTargetType.Client) ? new Resolver(context.ClientProject.ReleaseManifest.Root) : null;
               var gameResolver = context.Targets.HasFlag(ModificationTargetType.Game) ? new Resolver(context.GameProject.ReleaseManifest.Root) : null;
               var clientFirst = new[] { clientResolver, gameResolver };
               var gameFirst = new[] { gameResolver, clientResolver };

               var repository = new LocalRepository(modification.RootPath);
               using (repository.TakeLock())  {
                  string resolutionMetadataFilepath = repository.GetMetadataFilePath(RESOLUTION_METADATA_FILE_NAME);
                  using (var resolutionMetadata = new ResolutionMetadata(resolutionMetadataFilepath)) {
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
                           resolutionEntry = new ResolutionMetadata.ResolutionMetadataValue(null, Hash160.Zero, ModificationTargetType.Invalid);
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

               context.Task.UpdateCompleted();
            } catch (OperationCanceledException) {
               logger.Info(prefix + "reached operation cancelled exception");
            }
         }
         logger.Info(prefix + "exiting");
      }

      private class ResolutionContext
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