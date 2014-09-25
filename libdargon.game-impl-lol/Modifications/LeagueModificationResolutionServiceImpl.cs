using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.RADS;
using Dargon.Modifications;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class LeagueModificationResolutionServiceImpl : LeagueModificationResolutionService
   {
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

         var newTask = new ResolutionTask();

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
               logger.Info(prefix + "got task " + context.Task);
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