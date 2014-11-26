using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using Dargon.IO.RADS.Manifest;
using ItzWarty;
using NLog;
using System;
using System.Collections.Generic;

namespace Dargon.LeagueOfLegends.RADS
{
   public class RadsServiceImpl : RadsService
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly object synchronization = new object();
      private readonly Dictionary<uint, IReadOnlyList<RiotArchive>> archivesById = new Dictionary<uint, IReadOnlyList<RiotArchive>>();
      private readonly Dictionary<RiotProjectType, RiotProject> projectsByType = new Dictionary<RiotProjectType, RiotProject>();
      private readonly string solutionPath;
      private RiotArchiveLoader archiveLoader;
      public event EventHandler Suspending;
      public event EventHandler Resumed;

      public RadsServiceImpl(string solutionPath)
      {
         this.solutionPath = solutionPath;
      }

      public IRadsProjectReference GetProjectReference(RiotProjectType projectType)
      {
         lock (synchronization) {
            return new RadsProjectReference(GetProjectUnsafe(projectType));
         }
      }

      public IReadOnlyList<IRadsArchiveReference> GetArchiveReferences(uint version)
      {
         IReadOnlyList<RiotArchive> archives;
         if (!archivesById.TryGetValue(version, out archives)) {
            if (archiveLoader == null) {
               archiveLoader = new RiotArchiveLoader(solutionPath);
            }
            if (!archiveLoader.TryLoadArchives(version, out archives)) {
               throw new ArchiveNotFoundException(version);
            }
         }
         return Util.Generate(archives.Count, i => new RadsArchiveReference(archives[i]));
      }

      public IReadOnlyList<RiotArchive> GetArchivesUnsafe(uint version)
      {
         lock (synchronization) {
            IReadOnlyList<RiotArchive> archive;
            new RiotArchiveLoader(solutionPath).TryLoadArchives(version, out archive);
            return archive;
         }
      }

      public ReleaseManifest GetReleaseManifestUnsafe(RiotProjectType projectType) {
         lock (synchronization) {
            return new ReleaseManifestLoader().LoadProjectManifest(solutionPath, projectType);
         }
      }

      public RiotProject GetProjectUnsafe(RiotProjectType projectType) 
      {
         lock (synchronization) {
            RiotProject result;
            if (!projectsByType.TryGetValue(projectType, out result)) {
               result = new RiotProjectLoader(solutionPath).LoadProject(projectType);
            }
            return result;
         }
      }

      public void Suspend() 
      {
         lock (synchronization) {
            OnSuspending();

            archivesById.Clear();
            archiveLoader = null;
            // TODO: Invalidate all RADS Project/Archive References
         }
      }

      public void Resume() {
         lock (synchronization) {
            OnResumed();
         }
      }

      protected virtual void OnSuspending()
      {
         EventHandler handler = Suspending;
         if (handler != null) handler(this, EventArgs.Empty);
      }

      protected virtual void OnResumed()
      {
         EventHandler handler = Resumed;
         if (handler != null) handler(this, EventArgs.Empty);
      }
   }
}
