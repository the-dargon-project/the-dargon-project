using Dargon.FileSystem;
using Dargon.IO;
using Dargon.IO.RADS;
using Dargon.IO.RADS.Archives;
using Dargon.LeagueOfLegends.RADS;
using ItzWarty;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dargon.LeagueOfLegends.FileSystem
{
   public class RiotFileSystem : IFileSystem
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      private readonly RadsService radsService;
      private readonly RiotProjectType projectType;
      private readonly ConcurrentDictionary<IReadableDargonNode, InternalHandle> handlesByNode = new ConcurrentDictionary<IReadableDargonNode, InternalHandle>();
      private readonly Dictionary<uint, IRadsArchiveReference> archiveReferencesById = new Dictionary<uint, IRadsArchiveReference>();
      private readonly ReaderWriterLockSlim synchronization = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      private readonly object initializationLock = new object();
      private readonly List<SuspendedHandleContext> suspendedHandleContexts = new List<SuspendedHandleContext>(); 
      private volatile IRadsProjectReference projectReference;

      public RiotFileSystem(RadsService radsService, RiotProjectType projectType)
      {
         this.radsService = radsService;
         this.projectType = projectType;

         radsService.Resumed += HandleRadsServiceResumed;
         radsService.Suspending += HandleRadsServiceSuspending;
      }

      private void HandleRadsServiceResumed(object sender, EventArgs eventArgs)
      {
         LoadingOperation(() => {
            logger.Info("Resuming LoL File System " + this.projectType);

            // initialize underlying filesystem 
            if (suspendedHandleContexts.Any()) {
               GetProject();
            }

            foreach (var context in this.suspendedHandleContexts) {
               var handle = context.Handle;
               var path = context.Path;
               handle.EndReset(null); // this could be better with a path-to-node resolve method.
            }
         });
      }

      private void HandleRadsServiceSuspending(object sender, EventArgs eventArgs)
      {
         LoadingOperation(() => {
            logger.Info("Suspending LoL File System " + this.projectType);

            foreach (var kvp in handlesByNode) {
               var handle = kvp.Value;
               suspendedHandleContexts.Add(new SuspendedHandleContext(handle.Node.GetPath(), handle));
               handle.BeginReset();
            }
            handlesByNode.Clear();

            // Free reference to project so that rads service can free file handle
            if (projectReference != null) {
               projectReference.Dispose();
               projectReference = null;
            }

            foreach (var kvp in archiveReferencesById) {
               kvp.Value.Dispose();
            }
            archiveReferencesById.Clear();
         });
      }

      private T ReadOperation<T>(Func<T> body)
      {
         try {
            synchronization.EnterReadLock();
            return body();
         } finally {
            synchronization.ExitReadLock();
         }
      }

      private void LoadingOperation(Action body)
      {
         try {
            synchronization.EnterWriteLock();
            body();
         } finally {
            synchronization.ExitWriteLock();
         }
      }

      public IFileSystemHandle AllocateRootHandle() { return ReadOperation(AllocateRootHandleInternal); }

      private IFileSystemHandle AllocateRootHandleInternal() { return GetNodeHandle(GetProject().ReleaseManifest.Root); }

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
         try {
            archive = GetArchiveOrNull(asFile.ArchiveId);
         } catch (ArchiveNotFoundException) {
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

      // Assumes thread safety from ReadOperation
      private RiotProject GetProject() 
      {
         if (projectReference == null) {
            lock (initializationLock) {
               if (projectReference == null) {
                  projectReference = radsService.GetProjectReference(projectType);
               }
            }
         }
         return projectReference.Value; 
      }

      private RiotArchive GetArchiveOrNull(uint archiveId)
      {
         IRadsArchiveReference reference;
         if (!archiveReferencesById.TryGetValue(archiveId, out reference)) {
            lock (initializationLock) {
               if (!archiveReferencesById.TryGetValue(archiveId, out reference)) {
                  reference = radsService.GetArchiveReference(archiveId);
                  archiveReferencesById.Add(archiveId, reference);
               }
            }
         }
         return reference.Value;
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

   // public methods enforce thread safety if necessary

   // private methods assume thread safety from public method
}
