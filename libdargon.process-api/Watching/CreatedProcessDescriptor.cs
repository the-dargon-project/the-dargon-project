using System;

namespace Dargon.Processes.Watching {
   public class CreatedProcessDescriptor : IEquatable<CreatedProcessDescriptor> {
      private readonly string processName;
      private readonly int processId;
      private readonly int parentProcessId;

      public CreatedProcessDescriptor(string processName, int processId, int parentProcessId) {
         this.processName = processName;
         this.processId = processId;
         this.parentProcessId = parentProcessId;
      }

      public string ProcessName { get { return processName; } }
      public int ProcessId { get { return processId; } }
      public int ParentProcessId { get { return parentProcessId; } }

      public override bool Equals(object obj) {
         if (ReferenceEquals(null, obj)) return false;
         if (ReferenceEquals(this, obj)) return true;
         if (obj.GetType() != this.GetType()) return false;
         return Equals((CreatedProcessDescriptor)obj);
      }

      public bool Equals(CreatedProcessDescriptor other) {
         if (ReferenceEquals(null, other)) return false;
         if (ReferenceEquals(this, other)) return true;
         return string.Equals(processName, other.processName) && processId == other.processId && parentProcessId == other.parentProcessId;
      }

      public override int GetHashCode() {
         unchecked {
            int hashCode = (processName != null ? processName.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ processId;
            hashCode = (hashCode * 397) ^ parentProcessId;
            return hashCode;
         }
      }
   }
}
