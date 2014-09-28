using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dargon.Daemon;
using Dargon.LeagueOfLegends.RADS;
using Dargon.Modifications;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Modifications
{
   public abstract class LeagueModificationOperationServiceBase<TCancellableTaskInterface, TCancellableTaskImpl, TContext>
      where TCancellableTaskInterface : ITask
      where TCancellableTaskImpl : TCancellableTaskInterface, IManagableTask
      where TContext : ITaskContext<TCancellableTaskImpl>
   {
      protected readonly Logger logger;

      private readonly Dictionary<Guid, TCancellableTaskImpl> tasksByModificationLocalGuid = new Dictionary<Guid, TCancellableTaskImpl>();
      private readonly BlockingCollection<TContext> taskContextQueue = new BlockingCollection<TContext>(new ConcurrentQueue<TContext>());
      private readonly CancellationTokenSource shutdownCancellationTokenSource = new CancellationTokenSource();
      private readonly DaemonService daemonService;
      private readonly CancellationToken shutdownCancellationToken;
      private readonly Thread[] consumerThreads;

      public LeagueModificationOperationServiceBase(DaemonService daemonService)
      {
         logger = LogManager.GetLogger(GetType().FullName);

         this.daemonService = daemonService;
         this.shutdownCancellationToken = shutdownCancellationTokenSource.Token;

         this.consumerThreads = Util.Generate(
            Math.Max(1, Environment.ProcessorCount / 2),
            i => new Thread(() => ConsumerThreadStart(i)) { IsBackground =  true }.With(t => t.Start())
         );
      }

      protected abstract TContext CreateContext(TCancellableTaskImpl task, ModificationTargetType target);

      protected abstract void ProcessTaskContext(TContext context);

      protected void AddTask(Guid modificationGuid, TCancellableTaskImpl task, ModificationTargetType target)
      {
         TCancellableTaskImpl previousTask;
         if (tasksByModificationLocalGuid.TryGetValue(modificationGuid, out previousTask)) {
            previousTask.SetNext(task);
            previousTask.Cancel();
         }

         taskContextQueue.Add(CreateContext(task, target));
      }

      private void ConsumerThreadStart(int id)
      {
         var prefix = "t" + id + ": ";
         logger.Info(prefix + "at entry point");
         while (!shutdownCancellationToken.IsCancellationRequested) {
            try {
               logger.Info(prefix + "taking task context...");
               var context = taskContextQueue.Take(shutdownCancellationToken);

               logger.Info(prefix + "processing task context" + context);
               ProcessTaskContext(context);

               logger.Info(prefix + "processed task context" + context);
               context.Task.UpdateCompleted();
            } catch (OperationCanceledException) {
               logger.Info(prefix + "reached operation cancelled exception");
            }
         }
         logger.Info(prefix + "exiting");
      }
   }
}
