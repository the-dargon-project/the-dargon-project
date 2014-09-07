using System;
using System.Collections.Generic;

namespace Dargon.IO
{
   public interface IWritableDargonNode : IReadableDargonNode
   {
      new string Name { get; set; }
      new IWritableDargonNode Parent { get; set; }
      new IReadOnlyList<IWritableDargonNode> Children { get; }
      void AddComponent(Type componentInterface, object component);
      bool AddChild(IWritableDargonNode node);
      bool RemoveChild(IWritableDargonNode node);
   }
}
