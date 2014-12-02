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
using ItzWarty.Threading;
using NLog;

namespace Dargon.LeagueOfLegends.Modifications
{
   public abstract class LeagueModificationOperationServiceBase<TCancellableTaskInterface, TCancellableTaskImpl, TContext>
      where TCancellableTaskInterface : ITask
      where TCancellableTaskImpl : TCancellableTaskInterface, IManagableTask
      where TContext : ITaskContext<TCancellableTaskImpl>
   {
      protected readonly Logger logger;

      private readonly Dictionary<string, TCancellableTaskImpl> tasksByModificationName = new Dictionary<string, TCancellableTaskImpl>();
      private readonly DaemonService daemonService;
      private readonly ICancellationTokenSource shutdownCancellationTokenSource;
      private readonly ICancellationToken shutdownCancellationToken;
      private readonly TaskProcessor[] consumers;
      private readonly object previousTasksLock = new object();
      private int roundRobinCounter = 0;

      public LeagueModificationOperationServiceBase(IThreadingProxy threadingProxy, DaemonService daemonService)
      {
         logger = LogManager.GetLogger(GetType().FullName);

         this.daemonService = daemonService;
         this.shutdownCancellationTokenSource = threadingProxy.CreateCancellationTokenSource();
         this.shutdownCancellationToken = shutdownCancellationTokenSource.Token;

         this.consumers = Util.Generate(
            Math.Max(1, Environment.ProcessorCount / 2),
            i => new TaskProcessor(threadingProxy, i, shutdownCancellationToken, ProcessTaskContext)
         );
      }

      protected abstract TContext CreateContext(TCancellableTaskImpl task, ModificationTargetType target);

      protected abstract void ProcessTaskContext(TContext context);

      protected void AddTask(string modificationName, TCancellableTaskImpl task, ModificationTargetType target)
      {
         lock (previousTasksLock) {
            TCancellableTaskImpl previousTask;
            if (tasksByModificationName.TryGetValue(modificationName, out previousTask)) {
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

         private readonly IThreadingProxy threadingProxy;
         private readonly int id;
         private readonly ICancellationToken shutdownCancellationToken;
         private readonly Action<TContext> processTaskContext;
         private readonly Thread thread;
         private readonly ISemaphore semaphore;
         private readonly ConcurrentQueue<TContext> taskContextQueue = new ConcurrentQueue<TContext>();

         public TaskProcessor(IThreadingProxy threadingProxy, int id, ICancellationToken shutdownCancellationToken, Action<TContext> processTaskContext)
         {
            this.threadingProxy = threadingProxy;
            this.id = id;
            this.shutdownCancellationToken = shutdownCancellationToken;
            this.processTaskContext = processTaskContext;
            this.semaphore = threadingProxy.CreateSemaphore(0, int.MaxValue);

            this.thread = new Thread(ThreadStart) { IsBackground = true };
            this.thread.Start();
         }

         private void ThreadStart()
         {
            var prefix = "t" + id + ": ";
            logger.Info(prefix + "at entry point");
            while (!shutdownCancellationToken.IsCancellationRequested) {
               if (!semaphore.Wait(shutdownCancellationToken))
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
            semaphore.Release();
         }
      }
   }
}
