using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dargon.InjectedModule.Tasks
{
   public class LateInitializationTasklistProxy : ITasklist
   {
      private readonly CountdownEvent initializationLatch = new CountdownEvent(1);
      private ITasklist tasklist;

      public IEnumerator<ITask> GetEnumerator() { return GetRealTasklist().GetEnumerator(); }
      IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
      public int Count { get { return GetRealTasklist().Count; } }
      public ITask this[int index] { get { return GetRealTasklist()[index]; } }

      public void SetTasklist(ITasklist tasklist)
      {
         this.tasklist = tasklist;
         initializationLatch.Signal();
      }

      public void Wait() { initializationLatch.Wait(); }

      private IReadOnlyList<ITask> GetRealTasklist()
      {
         Wait();
         return tasklist;
      }
   }
}
