using System.Collections.Generic;

namespace Dargon.IO
{
    public interface IReadableDargonNode
    {
       string Name { get; }
       IReadableDargonNode Parent { get; }
       IReadOnlyList<IReadableDargonNode> Children { get; }
       T GetComponentOrNull<T>();
    }
}
