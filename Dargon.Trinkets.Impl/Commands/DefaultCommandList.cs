using System.Collections;
using System.Collections.Generic;
using Dargon.PortableObjects;
using ItzWarty;

namespace Dargon.Trinkets.Commands {
   public class DefaultCommandList : CommandList {
      private IList<Command> internalList;

      public DefaultCommandList() : this(new List<Command>()) { }

      public DefaultCommandList(IList<Command> internalList) {
         this.internalList = internalList;
      }

      public void Add(Command command) => internalList.Add(command);

      public void AddRange(CommandList other) {
         foreach (var command in other) {
            internalList.Add(command);
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteCollection(0, internalList, true);
      }

      public void Deserialize(IPofReader reader) {
         internalList = reader.ReadCollection<Command, List<Command>>(0, true);
      }

      public IEnumerator<Command> GetEnumerator() {
         return internalList.GetEnumerator();
      }

      public int Count => internalList.Count;

      public Command this[int index] => internalList[index];
   }
}
