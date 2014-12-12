using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Dargon.InjectedModule.Commands
{
   public class LateInitializationCommandListProxy : ICommandList
   {
      private readonly CountdownEvent initializationLatch = new CountdownEvent(1);
      private ICommandList commandList;

      public IEnumerator<ICommand> GetEnumerator() { return GetRealCommandList().GetEnumerator(); }
      IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
      public int Count { get { return GetRealCommandList().Count; } }
      public ICommand this[int index] { get { return GetRealCommandList()[index]; } }

      public void SetImplementation(ICommandList commandList)
      {
         this.commandList = commandList;
         initializationLatch.Signal();
      }

      public void Wait() { initializationLatch.Wait(); }

      private IReadOnlyList<ICommand> GetRealCommandList()
      {
         Wait();
         return commandList;
      }
   }
}
