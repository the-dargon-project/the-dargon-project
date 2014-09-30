using System;
using System.Collections.Generic;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.RADS
{
   public class RadsServiceImpl : RadsService
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly object synchronization = new object();
      private readonly Dictionary<uint, RiotArchive> archivesById = new Dictionary<uint, RiotArchive>();
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

      public IRadsArchiveReference GetArchiveReference(uint version) 
      {
         lock (synchronization) {
            RiotArchive archive;
            if (!archivesById.TryGetValue(version, out archive)) {
               if (archiveLoader == null) {
                  archiveLoader = new RiotArchiveLoader(solutionPath);
               }
               if (!archiveLoader.TryLoadArchive(version, out archive)) {
                  throw new ArchiveNotFoundException(version);
               }
            }
            return new RadsArchiveReference(archive);
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
