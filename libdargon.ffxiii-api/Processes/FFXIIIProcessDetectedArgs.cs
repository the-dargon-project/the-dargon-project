using Dargon.Processes.Watching;

namespace Dargon.FinalFantasyXIII.Processes
{
   public delegate void FFXIIIProcessDetectedHandler(FFXIIIProcessDetectedArgs args);
   public class FFXIIIProcessDetectedArgs
   {
      public FFXIIIProcessType ProcessType { get; private set; }
      public CreatedProcessDescriptor ProcessDescriptor { get; private set; }

      public FFXIIIProcessDetectedArgs(
         FFXIIIProcessType ffxiiiProcessType,
         CreatedProcessDescriptor createdProcessDescriptor)
      {
         ProcessType = ffxiiiProcessType;
         ProcessDescriptor = createdProcessDescriptor;
      }
   }
}
