using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Dargon.PortableObjects;

namespace Dargon.Trinkets.Commands
{
   public class CommandListProxy : CommandList
   {
      private readonly CountdownEvent initializationLatch = new CountdownEvent(1);
      private CommandList commandList;

      public IEnumerator<Command> GetEnumerator() { return GetRealCommandList().GetEnumerator(); }
      public int Count { get { return GetRealCommandList().Count; } }
      public Command this[int index] { get { return GetRealCommandList()[index]; } }

      public void SetImplementation(CommandList commandList)
      {
         this.commandList = commandList;
         initializationLatch.Signal();
      }

      public void Wait() { initializationLatch.Wait(); }

      private CommandList GetRealCommandList()
      {
         Wait();
         return commandList;
      }

      public void Serialize(IPofWriter writer) {
         Wait();
         writer.WriteObject(0, this.commandList);
      }

      public void Deserialize(IPofReader reader) {
         this.commandList = reader.ReadObject<CommandList>(0);
         this.initializationLatch.Signal();
      }
   }
}
