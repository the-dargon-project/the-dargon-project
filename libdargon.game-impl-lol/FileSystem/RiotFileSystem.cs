using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using ItzWarty;
using NLog;

namespace Dargon.FileSystem
{
   public class RiotFileSystem : IFileSystem
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly string solutionPath;
      private readonly RiotProjectType projectType;
      private readonly ConcurrentDictionary<IReadableDargonNode, InternalHandle> handlesByNode = new ConcurrentDictionary<IReadableDargonNode, InternalHandle>();
      private readonly Dictionary<uint, RiotArchive> archivesById = new Dictionary<uint, RiotArchive>();   
      private RiotProject project;

      public RiotFileSystem(string solutionPath, RiotProjectType projectType)
      {
         this.solutionPath = solutionPath;
         this.projectType = projectType;
      }

      private void Initialize()
      {
         project = new RiotSolutionLoader().Load(solutionPath, projectType).ProjectsByType[projectType];
         var manifest = project.ReleaseManifest;
         var archiveIds = new HashSet<uint>();
         foreach (var file in manifest.Files) {
            archiveIds.Add(file.ArchiveId);
         }

         var riotArchiveLoader = new RiotArchiveLoader(solutionPath);
         var successfullyLoadedArchives = new SortedSet<uint>();
         var unsuccessfullyLoadedArchives = new SortedSet<uint>();
         foreach (var archiveId in archiveIds) {
            RiotArchive archive;
            if (riotArchiveLoader.TryLoadArchive(archiveId, out archive)) {
               archivesById.Add(archiveId, archive);
               successfullyLoadedArchives.Add(archiveId);
            } else {
               unsuccessfullyLoadedArchives.Add(archiveId);
            }
         }
         logger.Info("Successfully loaded {0} archives: {1}".F(successfullyLoadedArchives.Count, successfullyLoadedArchives.Join(", ")));
         logger.Warn("Failed to load {0} archives: {1}".F(unsuccessfullyLoadedArchives.Count, unsuccessfullyLoadedArchives.Join(", ")));
      }

      public IFileSystemHandle AllocateRootHandle() { 
         return GetNodeHandle(project.ReleaseManifest.Root);
      }

      public IoResult AllocateChildrenHandles(IFileSystemHandle handle, out IFileSystemHandle[] childHandles)
      {
         var internalHandle = handle as InternalHandle;

         if (internalHandle == null || !internalHandle.IsValid) {
            childHandles = null;
            return IoResult.InvalidHandle;
         }

         var children = internalHandle.Node.Children;
         var results = new IFileSystemHandle[children.Count];
         var i = 0;
         foreach (var child in children) {
            results[i++] = GetNodeHandle(child);
         }
         childHandles = results;
         return IoResult.Success;
      }

      public IoResult ReadAllBytes(IFileSystemHandle handle, out byte[] bytes)
      {
         var internalHandle = handle as InternalHandle;

         if (internalHandle == null || !internalHandle.IsValid) {
            bytes = null;
            return IoResult.InvalidHandle;
         }

         var asFile = internalHandle.Node as ReleaseManifestFileEntry;
         if (asFile == null) { // we have a directory
            bytes = null;
            return IoResult.InvalidOperation;
         }

         RiotArchive archive;
         if (!archivesById.TryGetValue(asFile.ArchiveId, out archive)) {
            bytes = null;
            return IoResult.Unavailable;
         }

         var entry = archive.GetDirectoryFile().GetFileList().GetFileEntryOrNull(internalHandle.Node.GetPath());
         if (entry == null) {
            bytes = null;
            return IoResult.NotFound;
         }

         bytes = entry.GetContent();
         return IoResult.Success;
      }

      public void FreeHandle(IFileSystemHandle handle) { 
         var internalHandle = handle as InternalHandle;
         if (internalHandle != null) {
            internalHandle.DecrementReferenceCount();
         }
      }
      public void FreeHandles(IEnumerable<IFileSystemHandle> handles) { foreach(var handle in handles) FreeHandle(handle); }
      public IoResult GetPath(IFileSystemHandle handle, out string path)
      {
         var internalHandle = handle as InternalHandle;

         if (internalHandle == null || !internalHandle.IsValid)
         {
            path = null;
            return IoResult.InvalidHandle;
         }

         path = internalHandle.Node.GetPath(); 
         return IoResult.Success;
      }

      public void Suspend() { throw new NotImplementedException(); }
      public void Resume() { throw new NotImplementedException(); }

      private InternalHandle GetNodeHandle(IReadableDargonNode node) {
         return handlesByNode.AddOrUpdate(node, n => new InternalHandle(n), (n, h) => { h.IncrementReferenceCount(); return h; });
      }

      private class InternalHandle : IFileSystemHandle
      {
         private int referenceCount; 

         public bool IsValid { get; set; }
         public int ReferenceCount { get { return referenceCount; } }
         public IReadableDargonNode Node { get; private set; }

         public InternalHandle(IReadableDargonNode node) {
            this.Node = node;
            this.IsValid = true;
         }

         public int IncrementReferenceCount() { return Interlocked.Increment(ref referenceCount); }
         public int DecrementReferenceCount() { return Interlocked.Decrement(ref referenceCount); }

         /// <summary>
         /// Returns a string that represents the current object.
         /// </summary>
         /// <returns>
         /// A string that represents the current object.
         /// </returns>
         public override string ToString() { return "[RFS Handle to " + Node.GetPath() + " ]"; }
      }
   }
}
