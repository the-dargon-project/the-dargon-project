using System.Collections;
using System.Collections.Generic;
using Dargon.PortableObjects;

namespace Dargon.Trinkets.Commands {
   public interface CommandList : IPortableObject {
      IEnumerator<Command> GetEnumerator();
      int Count { get; }
     Command this[int index] { get; }
   }
}
