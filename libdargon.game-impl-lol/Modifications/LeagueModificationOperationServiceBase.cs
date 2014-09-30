using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
      private readonly CancellationTokenSource shutdownCancellationTokenSource = new CancellationTokenSource();
      private readonly DaemonService daemonService;
      private readonly CancellationToken shutdownCancellationToken;
      private readonly TaskProcessor[] consumers;
      private readonly object previousTasksLock = new object();
      private int roundRobinCounter = 0;

      public LeagueModificationOperationServiceBase(DaemonService daemonService)
      {
         logger = LogManager.GetLogger(GetType().FullName);

         this.daemonService = daemonService;
         this.shutdownCancellationToken = shutdownCancellationTokenSource.Token;

         this.consumers = Util.Generate(
            Math.Max(1, Environment.ProcessorCount / 2),
            i => new TaskProcessor(i, shutdownCancellationToken, ProcessTaskContext)
         );
      }

      protected abstract TContext CreateContext(TCancellableTaskImpl task, ModificationTargetType target);

      protected abstract void ProcessTaskContext(TContext context);

      protected void AddTask(Guid modificationGuid, TCancellableTaskImpl task, ModificationTargetType target)
      {
         lock (previousTasksLock) {
            TCancellableTaskImpl previousTask;
            if (tasksByModificationLocalGuid.TryGetValue(modificationGuid, out previousTask)) {
               previousTask.SetNext(task);
               previousTask.Cancel();
            }
         }

         logger.Info("Enqueuing task " + task);
         int consumerId = Interlocked.Increment(ref roundRobinCounter);
         var consumer = consumers[consumerId % consumers.Length];
         consumer.EnqueueTaskContext(CreateContext(task, target));
         logger.Info("Done enqueuing task " + task);
      }

      private class TaskProcessor
      {
         private static readonly Logger logger = LogManager.GetCurrentClassLogger();

         private readonly int id;
         private readonly CancellationToken shutdownCancellationToken;
         private readonly Action<TContext> processTaskContext;
         private readonly Thread thread;
         private readonly Semaphore semaphore = new Semaphore(0, Int32.MaxValue);
         private readonly ConcurrentQueue<TContext> taskContextQueue = new ConcurrentQueue<TContext>();

         public TaskProcessor(int id, CancellationToken shutdownCancellationToken, Action<TContext> processTaskContext)
         {
            this.id = id;
            this.shutdownCancellationToken = shutdownCancellationToken;
            this.processTaskContext = processTaskContext;

            this.thread = new Thread(ThreadStart) { IsBackground = true };
            this.thread.Start();
         }

         private void ThreadStart()
         {
            var prefix = "t" + id + ": ";
            logger.Info(prefix + "at entry point");
            while (!shutdownCancellationToken.IsCancellationRequested) {
               if (!semaphore.WaitOne(10000))
                  continue;

               TContext context;
               while (!taskContextQueue.TryDequeue(out context)) ;

               logger.Info(prefix + "taking task context...");

               if (!shutdownCancellationToken.IsCancellationRequested) {
                  if (context.Task.Status != Status.Cancelled) {
                     logger.Info(prefix + "processing task context" + context);
                     processTaskContext(context);

                     logger.Info(prefix + "processed task context" + context);
                     context.Task.UpdateCompleted();
                  }
               }
            }
            logger.Info(prefix + "exiting");
         }

         internal void EnqueueTaskContext(TContext context)
         {
            if (context == null)
               throw new ArgumentNullException("context");

            taskContextQueue.Enqueue(context);
            Thread.MemoryBarrier();
            semaphore.Release();
         }
      }
   }
}
