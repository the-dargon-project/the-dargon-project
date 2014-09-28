using System.Threading;
using Dargon.Modifications;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class ResolutionTask : ManagableTask, IResolutionTask
   {
      private readonly IModification modification;

      public ResolutionTask(IModification modification) : base() { this.modification = modification; }

      public IModification Modification { get { return modification; } }
   }

   public class ManagableTask : IManagableTask
   {
      private readonly CountdownEvent terminationEvent = new CountdownEvent(1);
      private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      private readonly CancellationToken cancellationToken;

      private Status status;
      private int successCount;
      private int failCount;
      private int totalCount;
      private ITask nextTask;

      public ManagableTask()
      {
         cancellationToken = cancellationTokenSource.Token;
         status = Status.Pending;
      }

      public Status Status { get { return status; } }
      public int SuccessCount { get { return successCount; } }
      public int FailCount { get { return failCount; } }
      public int TotalCount { get { return totalCount; } }
      public ITask NextTask { get { return nextTask; } }
      public CancellationToken CancellationToken { get { return cancellationToken; } }

      public void SetNext(ITask newTask) { this.nextTask = newTask; }

      public void UpdateProgress(Status status, int successCount, int failCount, int totalCount)
      {
         lock (this) {
            this.status = status;
            this.successCount = successCount;
            this.failCount = failCount;
            this.totalCount = totalCount;
         }
      }

      public void UpdateCompleted()
      {
         lock (this) {
            this.status = Status.Completed;
            SignalTermination();
         }
      }

      public void Cancel()
      {
         lock (this) {
            if (this.status != Status.Completed) {
               this.status = Status.Cancelled;
               cancellationTokenSource.Cancel();
               SignalTermination();
            }
         }
      }

      private void SignalTermination() { terminationEvent.Signal(); }

      public void WaitForTermination() { terminationEvent.Wait(); }
   }
}