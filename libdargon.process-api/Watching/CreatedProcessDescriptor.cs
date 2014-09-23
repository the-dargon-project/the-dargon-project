namespace Dargon.Processes.Watching
{
   public class CreatedProcessDescriptor
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
   }
}
