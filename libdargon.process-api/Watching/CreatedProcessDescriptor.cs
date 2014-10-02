using System;

namespace Dargon.Processes.Watching
{
   public class CreatedProcessDescriptor : IEquatable<CreatedProcessDescriptor>
   {
      private readonly string processName;
      private readonly int processId;
      private readonly int parentProcessId;

      public CreatedProcessDescriptor(string processName, int processId, int parentProcessId)
      {
         this.processName = processName;
         this.processId = processId;
         this.parentProcessId = parentProcessId;
      }

      public string ProcessName { get { return processName; } }
      public int ProcessId { get { return processId; } }
      public int ParentProcessId { get { return parentProcessId; } }

      public override bool Equals(object obj) { return obj is CreatedProcessDescriptor && Equals((CreatedProcessDescriptor)obj); }
      public bool Equals(CreatedProcessDescriptor other) { return other.processName.Equals(this.processName, StringComparison.OrdinalIgnoreCase) && other.processId == processId && other.parentProcessId == parentProcessId; }
   }
}
