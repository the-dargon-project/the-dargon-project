using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using Dargon.IO.RADS.Manifest;
using System;
using System.Collections.Generic;

namespace Dargon.LeagueOfLegends.RADS
{
   public interface RadsService
   {
      event EventHandler Suspending;
      event EventHandler Resumed;
      
      IRadsProjectReference GetProjectReference(RiotProjectType projectType);
      IReadOnlyList<IRadsArchiveReference> GetArchiveReferences(uint version);

      ReleaseManifest GetReleaseManifestUnsafe(RiotProjectType projectType);
      RiotProject GetProjectUnsafe(RiotProjectType projectType);
      IReadOnlyList<RiotArchive> GetArchivesUnsafe(uint version);

      void Suspend();
      void Resume();
   }
}