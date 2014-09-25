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
      private readonly RiotSolutionLoader solutionLoader = new RiotSolutionLoader();
      private readonly Dictionary<uint, RiotArchive> archivesById = new Dictionary<uint, RiotArchive>();
      private readonly string solutionPath;
      private RiotSolution solution = null;
      private RiotArchiveLoader archiveLoader = null;
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
            if (solution == null) {
               var loader = new RiotSolutionLoader();
               solution = loader.Load(solutionPath, RiotProjectType.AirClient | RiotProjectType.GameClient);
            }
            return solution.ProjectsByType[projectType];
         }
      }

      public void Suspend() 
      {
         lock (synchronization) {
            OnSuspending();

            archivesById.Clear();
            solution = null;
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
