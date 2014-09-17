using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Dargon.FileSystem;
using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.FileSystem
{
   // public methods enforce thread safety if necessary
   // private methods assume thread safety from public method
   public class RiotFileSystem : IFileSystem
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly string solutionPath;
      private readonly RiotProjectType projectType;
      private readonly ConcurrentDictionary<IReadableDargonNode, InternalHandle> handlesByNode = new ConcurrentDictionary<IReadableDargonNode, InternalHandle>();
      private readonly Dictionary<uint, RiotArchive> archivesById = new Dictionary<uint, RiotArchive>();
      private readonly ReaderWriterLockSlim synchronization = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      private readonly object initializationLock = new object();
      private readonly List<SuspendedHandleContext> suspendedHandleContexts = new List<SuspendedHandleContext>(); 
      private bool initialized = false;
      private int suspensionCount = 0;
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

      // assumes 
      private void EnsureFileSystemInitialized()
      {
         if (!initialized) {
            lock (initializationLock) {
               if (!initialized) {
                  Initialize();
                  initialized = true;
               }
            }
         }
      }

      private T ReadOperation<T>(Func<T> body)
      {
         try {
            synchronization.EnterReadLock();
            EnsureFileSystemInitialized();
            return body();
         } finally {
            synchronization.ExitReadLock();
         }
      }

      private void SuspensionOperation(Action body)
      {
         try {
            synchronization.EnterWriteLock();
            body();
         } finally {
            synchronization.ExitWriteLock();
         }
      }

      public IFileSystemHandle AllocateRootHandle() { return ReadOperation(AllocateRootHandleInternal); }

      private IFileSystemHandle AllocateRootHandleInternal() { return GetNodeHandle(project.ReleaseManifest.Root); }

      public IoResult AllocateChildrenHandles(IFileSystemHandle handle, out IFileSystemHandle[] childHandles)
      {
         var result = ReadOperation(() => AllocateChildrenHandlesInternal(handle));
         childHandles = result.Item2;
         return result.Item1;
      }

      private Tuple<IoResult, IFileSystemHandle[]> AllocateChildrenHandlesInternal(IFileSystemHandle handle)
      {
         var internalHandle = handle as InternalHandle;

         if (internalHandle == null || internalHandle.State == HandleState.Invalidated || internalHandle.State == HandleState.Disposed) {
            return new Tuple<IoResult, IFileSystemHandle[]>(IoResult.InvalidHandle, null);
         }

         var children = internalHandle.Node.Children;
         var results = new IFileSystemHandle[children.Count];
         var i = 0;
         foreach (var child in children) {
            results[i++] = GetNodeHandle(child);
         }
         return new Tuple<IoResult, IFileSystemHandle[]>(IoResult.Success, results);
      }

      public IoResult ReadAllBytes(IFileSystemHandle handle, out byte[] bytes)
      {
         var result = ReadOperation(() => ReadAllBytesInternal(handle));
         bytes = result.Item2;
         return result.Item1;
      }

      public Tuple<IoResult, byte[]> ReadAllBytesInternal(IFileSystemHandle handle)
      {
         var internalHandle = handle as InternalHandle;

         if (internalHandle == null || internalHandle.State == HandleState.Invalidated || internalHandle.State == HandleState.Disposed) {
            return new Tuple<IoResult, byte[]>(IoResult.InvalidHandle, null);
         }

         var asFile = internalHandle.Node as ReleaseManifestFileEntry;
         if (asFile == null) {
            // we have a directory
            return new Tuple<IoResult, byte[]>(IoResult.InvalidOperation, null);
         }

         RiotArchive archive;
         if (!archivesById.TryGetValue(asFile.ArchiveId, out archive)) {
            return new Tuple<IoResult, byte[]>(IoResult.Unavailable, null);
         }

         var entry = archive.GetDirectoryFile().GetFileList().GetFileEntryOrNull(internalHandle.Node.GetPath());
         if (entry == null) {
            return new Tuple<IoResult, byte[]>(IoResult.NotFound, null);
         }

         return new Tuple<IoResult, byte[]>(IoResult.Success, entry.GetContent());
      }

      public void FreeHandle(IFileSystemHandle handle)
      {
         var internalHandle = handle as InternalHandle;
         if (internalHandle != null) {
            var referenceCount = internalHandle.DecrementReferenceCount();
            if (referenceCount == 0) {
               lock (internalHandle) {
                  if (internalHandle.GetReferenceCount() == 0) {
                     internalHandle.Invalidate();
                  }
               }
            }
         }
      }

      public void FreeHandles(IEnumerable<IFileSystemHandle> handles) { foreach (var handle in handles) FreeHandle(handle); }

      public IoResult GetPath(IFileSystemHandle handle, out string path)
      {
         var internalHandle = handle as InternalHandle;

         if (internalHandle == null || internalHandle.State == HandleState.Invalidated || internalHandle.State == HandleState.Disposed) {
            path = null;
            return IoResult.InvalidHandle;
         }

         path = internalHandle.Node.GetPath();
         return IoResult.Success;
      }

      public void Suspend() { SuspensionOperation(SuspendInternal); }

      private void SuspendInternal()
      {
         logger.Info("Suspending LoL File System " + this.projectType);
         if (Interlocked.Increment(ref suspensionCount) == 1) {
            foreach (var kvp in handlesByNode) {
               var handle = kvp.Value;
               suspendedHandleContexts.Add(new SuspendedHandleContext(handle.Node.GetPath(), handle));
               handle.BeginReset();
            }
            handlesByNode.Clear();
         }
      }

      public void Resume() { SuspensionOperation(ResumeInternal); }

      private void ResumeInternal() { 
         logger.Info("Resuming LoL File System " + this.projectType);
         if (Interlocked.Decrement(ref suspensionCount) == 0) {
            if (suspendedHandleContexts.Any()) {
               Initialize(); // initialize underlying filesystem
            }

            foreach (var context in this.suspendedHandleContexts) {
               var handle = context.Handle;
               var path = context.Path;
               handle.EndReset(null); // this could be better with a path-to-node resolve method.
            }
         }
      }

      private InternalHandle GetNodeHandle(IReadableDargonNode node)
      {
         return handlesByNode.AddOrUpdate(node, n => new InternalHandle(n), (n, h) => {
            h.IncrementReferenceCount();
            return h;
         });
      }

      private class InternalHandle : IFileSystemHandle
      {
         private IReadableDargonNode node;
         private HandleState state;
         private int referenceCount;

         public InternalHandle(IReadableDargonNode node)
         {
            this.node = node;
            this.state = HandleState.Valid;
            this.referenceCount = 0;
         }

         public HandleState State { get { return state; } }
         public IReadableDargonNode Node { get { return node; } }

         public int GetReferenceCount() { return referenceCount; }
         public int IncrementReferenceCount() { return Interlocked.Increment(ref referenceCount); }
         public int DecrementReferenceCount() { return Interlocked.Decrement(ref referenceCount); }

         public void Invalidate() { state = HandleState.Invalidated; }

         public void BeginReset()
         {
            node = null;
            state = HandleState.Reset;
         }

         public void EndReset(IReadableDargonNode node)
         {
            this.node = node;
            this.state = node == null ? HandleState.Invalidated : HandleState.Valid;
         }

         public override string ToString() { return "[RFS Handle to " + Node.GetPath() + " ]"; }
      }

      private class SuspendedHandleContext
      {
         private readonly string path;
         private readonly InternalHandle handle;

         public SuspendedHandleContext(string path, InternalHandle handle)
         {
            this.path = path;
            this.handle = handle;
         }

         public string Path { get { return path; } }
         public InternalHandle Handle { get { return handle; } }
      }
   }
}
