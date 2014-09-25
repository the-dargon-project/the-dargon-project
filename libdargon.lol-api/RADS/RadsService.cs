using System;
using Dargon.IO.RADS;

namespace Dargon.LeagueOfLegends.RADS
{
   public interface RadsService
   {
      event EventHandler Suspending;
      event EventHandler Resumed;
      
      IRadsProjectReference GetProjectReference(RiotProjectType projectType);
      IRadsArchiveReference GetArchiveReference(uint version);
      
      RiotProject GetProjectUnsafe(RiotProjectType projectType);

      void Suspend();
      void Resume();
   }
}