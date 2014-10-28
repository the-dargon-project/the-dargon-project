using System;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;

namespace Dargon.LeagueOfLegends.RADS
{
   public interface RadsService
   {
      event EventHandler Suspending;
      event EventHandler Resumed;
      
      IRadsProjectReference GetProjectReference(RiotProjectType projectType);
      IRadsArchiveReference GetArchiveReference(uint version);

      ReleaseManifest GetReleaseManifestUnsafe(RiotProjectType projectType);
      RiotProject GetProjectUnsafe(RiotProjectType projectType);
      RiotArchive GetArchiveUnsafe(uint version);

      void Suspend();
      void Resume();
   }
}