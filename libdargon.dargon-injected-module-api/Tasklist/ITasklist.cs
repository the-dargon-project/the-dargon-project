using System.Collections.Generic;

namespace Dargon.InjectedModule.Tasklist
{
   public interface ITasklist : IReadOnlyList<ITask>
   {
   }

   public interface ITask
   {
      string Type { get; }
      byte[] Data { get; }
   }
}
