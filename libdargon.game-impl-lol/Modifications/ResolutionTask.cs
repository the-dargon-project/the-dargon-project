using System.Threading;

namespace Dargon.LeagueOfLegends.Modifications
{
   public class ResolutionTask : IResolutionTask
   {
      private readonly CountdownEvent terminationEvent = new CountdownEvent(1);
      private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
      private readonly CancellationToken cancellationToken;

      private ResolutionStatus status;
      private int filesResolved;
      private int filesUnresolved;
      private int fileCount;
      private IResolutionTask nextTask;

      public ResolutionTask()
      {
         cancellationToken = cancellationTokenSource.Token;
         status = ResolutionStatus.Pending;
      }

      public ResolutionStatus Status { get { return status; } }
      public int FilesResolved { get { return filesResolved; } }
      public int FileCount { get { return fileCount; } }
      public IResolutionTask NextTask { get { return nextTask; } }
      public CancellationToken CancellationToken { get { return cancellationToken; } }

      public void SetNext(ResolutionTask newTask) { this.nextTask = newTask; }

      public void UpdateProgress(ResolutionStatus status, int filesResolved, int filesUnresolved, int fileCount)
      {
         lock (this)
         {
            this.status = status;
            this.filesResolved = filesResolved;
            this.filesUnresolved = filesUnresolved;
            this.fileCount = fileCount;
         }
      }

      public void UpdateCompleted()
      {
         lock (this)
         {
            this.status = ResolutionStatus.Completed;
            SignalTermination();
         }
      }

      public void Cancel()
      {
         lock (this)
         {
            if (this.status != ResolutionStatus.Completed)
            {
               this.status = ResolutionStatus.Cancelled;
               cancellationTokenSource.Cancel();
               SignalTermination();
            }
         }
      }

      private void SignalTermination() { terminationEvent.Signal(); }

      public void WaitForTermination() { terminationEvent.Wait(); }
   }
}