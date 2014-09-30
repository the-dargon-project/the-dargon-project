using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dargon.InjectedModule.Tasklist
{
   public class LateInitializationTasklist : ITasklist
   {
      private readonly CountdownEvent initializationLatch = new CountdownEvent(1);
      private IReadOnlyList<ITask> result;

      public IEnumerator<ITask> GetEnumerator() { return Get().GetEnumerator(); }
      IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
      public int Count { get { return Get().Count; } }
      public ITask this[int index] { get { return Get()[index]; } }

      public void SetResult(IReadOnlyList<ITask> result)
      {
         this.result = result;
         initializationLatch.Signal();
      }

      public void Wait() { initializationLatch.Wait(); }

      public IReadOnlyList<ITask> Get()
      {
         Wait();
         return result;
      }
   }
}
