using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ItzWarty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.Patcher
{
   public interface IRepository
   {
      IRevision GetRevisionOrNull(Guid guid);
   }

   public class LocalRepository
   {
      private const string PATH_DELIMITER = "/";

      private readonly string path;
      private readonly string dpmPath;
      private readonly ObjectStore objectStore;
      private readonly FileLock repositoryLock;
      private readonly IndexProvider indexProvider;
      private readonly DpmSerializer serializer = new DpmSerializer();

      public LocalRepository(string path)
      {
         this.path = path;
         this.dpmPath = Path.Combine(path, ".dpm");
         this.objectStore = new ObjectStore(Path.Combine(dpmPath, "objects"));
         this.repositoryLock = new FileLock(Path.Combine(dpmPath, "LOCK"));
         this.indexProvider = new IndexProvider(Path.Combine(dpmPath, "LAST_MODIFIED"));
      }

      public void TrackFile(string internalPath)
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var entry = index.GetValueOrNull(internalPath);
            if (entry == null) {
               var hash = objectStore.Put(File.ReadAllBytes(GetAbsolutePath(internalPath)));
               index.Set(internalPath, new IndexEntry(GetTrueLastModified(internalPath), hash, IndexEntryFlags.Added));
            }
         }
      }

      public void Untrack(string internalPath, bool deletePhysically = false)
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var entry = index.GetValueOrNull(internalPath);
            if (entry != null) {
               if (entry.Flags.HasFlag(IndexEntryFlags.Directory)) {

               } else {
                  if (entry.Flags.HasFlag(IndexEntryFlags.Added)) {
                     index.Remove(internalPath);
                  } else {
                     entry.Flags |= IndexEntryFlags.Removed;
                     index.Set(internalPath, entry);
                  }
                  if (deletePhysically) {
                     File.Delete(GetAbsolutePath(internalPath));
                  }
               }
            }
         }
      }

      public void Commit()
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var changedEntries = EnumerateChangedEntries();
            var changedEntriesByParentAndName = from entry in changedEntries
                                                let breadcrumbs = entry.Key.Split('/')
                                                let parentPath = breadcrumbs.SubArray(0, breadcrumbs.Length - 1).Join(PATH_DELIMITER)
                                                let name = breadcrumbs[breadcrumbs.Length - 1]
                                                select new { Name = name, ParentPath = parentPath, Path = entry.Key, Value = entry.Value };
            var changedEntriesGroupedByParent = changedEntriesByParentAndName.GroupBy(x => x.ParentPath);
            var changedDirectoriesBinnedByParent = new MultiValueDictionary<string, Tuple<string, >>();
            foreach (var bin in changedEntriesGroupedByParent) {
               var childNamesByHash = new Dictionary<string, Hash160>();
               var removedChildNames = new HashSet<string>();
               foreach (var child in bin) {
                  var entry = index.GetValueOrNull(child.Path);
                  if (entry.Flags.HasFlag(IndexEntryFlags.Added)) {
                     childNamesByHash.Add(child.Name, entry.RevisionHash);
                  } else if (entry.Flags.HasFlag(IndexEntryFlags.Removed)) {
                     removedChildNames.Add(child.Name);
                  } else {
                     entry.RevisionHash = objectStore.Put(File.ReadAllBytes(GetAbsolutePath(child.Path)));
                     index.Set(child.Path, entry);
                     childNamesByHash.Add(child.Name, entry.RevisionHash);
                  }
               }
               UpdateModifiedFilesParent(bin.Key, childNamesByHash, removedChildNames);
            }
         }
      }

      public void UpdateModifiedFilesParent(string directoryInternalPath, IEnumerable<KeyValuePair<string, Hash160>> childNamesByHash, HashSet<string> removedChildNames)
      {

      }

      public void UpdateModifiedDirectoryParent(string childInternalPath, Hash160 childObjectHash) {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            bool done = false;
            while (!done) {
               var breadcrumbs = childInternalPath.Split(PATH_DELIMITER);
               var childName = breadcrumbs[breadcrumbs.Length - 1];
               var parentInternalPath = BuildPath(breadcrumbs.SubArray(0, breadcrumbs.Length - 1));

               // Update parent entry to point to new child hash
               var parentEntry = index.GetValueOrNull(parentInternalPath);
               var directory = serializer.DeserializeDirectoryRevision(parentEntry.RevisionHash, objectStore.Get(parentEntry.RevisionHash));
               directory.Children[childName] = childObjectHash;
               parentEntry.RevisionHash = objectStore.Put(serializer.SerializeDirectoryRevision(directory));
               index.Set(parentInternalPath, parentEntry);

               // prep for next iteration
               childInternalPath = parentInternalPath;
               childObjectHash = parentEntry.RevisionHash;

               if (breadcrumbs.Length < 2) {
                  done = true;
               }
            }
         }
      }

      public List<KeyValuePair<string, IndexEntry>> EnumerateChangedEntries()
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var entries = index.Enumerate();
            var changedEntries = new List<KeyValuePair<string, IndexEntry>>(); 
            foreach (var entry in entries) {
               if (entry.Value.Flags.HasFlag(IndexEntryFlags.Added) || entry.Value.Flags.HasFlag(IndexEntryFlags.Removed)) {
                  changedEntries.Add(entry);
               } else if (GetTrueLastModified(entry.Key) != entry.Value.LastModified) {
                  var fileRevision = serializer.DeserializeFileRevision(entry.Value.RevisionHash, objectStore.Get(entry.Value.RevisionHash));
                  var fileRevisionData = fileRevision.Data;
                  if (fileRevisionData.Length != new FileInfo(GetAbsolutePath(entry.Key)).Length) {
                     changedEntries.Add(entry);
                  } else {
                     var diskContent = File.ReadAllBytes(GetAbsolutePath(entry.Key));
                     bool equal = true;
                     for (var i = 0; i < diskContent.Length && equal; i++) {
                        equal &= fileRevisionData[i] == diskContent[i];
                     }
                     if (equal) {
                        entry.Value.LastModified = GetTrueLastModified(entry.Key);
                        index.Set(entry.Key, entry.Value);
                     } else {
                        changedEntries.Add(entry);
                     }
                  }
               }
            }
            return changedEntries;
         }
      }

      public void ResetRepository(Hash160 hash)
      {
         using (repositoryLock.Take()) {
            ResetDirectory("", hash);
         }
      }

      private void ResetDirectory(string internalPath, Hash160 hash)
      {
         using (repositoryLock.Take()) {
            var revision = GetDirectoryRevision(internalPath, hash);
            ResetDirectoryHelper(internalPath, revision);
         }
      }
      
      private void ResetHelper(string internalPath, Hash160 hash) { 
         var blob = objectStore.Get(hash);

         if (serializer.IsDirectoryRevision(blob)) {
            ResetDirectoryHelper(internalPath, serializer.DeserializeDirectoryRevision(hash, blob));
         } else if (serializer.IsFileRevision(blob)) {
            ResetFileHelper(internalPath, serializer.DeserializeFileRevision(hash, blob));
         }
      }

      private void ResetDirectoryHelper(string internalPath, DirectoryRevision newRevision) {
         using (var index = indexProvider.Take()) {
            var lastModified = GetTrueLastModified(internalPath);
            var entry = index.GetValueOrNull(internalPath);
            var oldRevision = GetDirectoryRevision(internalPath, entry.RevisionHash);
            if (lastModified != entry.LastModified || newRevision.Hash != entry.RevisionHash) {
               // Update subdirectories no longer existing in new revision
               foreach (var child in oldRevision.Children.Where(c => newRevision.Children.None(kvp => kvp.Key == c.Key))) {
                  ResetDeleteHelper(BuildPath(internalPath, child.Key));
               }

               // Update subdirectories and files to match target revision
               foreach (var child in newRevision.Children) {
                  ResetHelper(BuildPath(internalPath, child.Key), child.Value);
               }
            }
            entry.LastModified = GetTrueLastModified(internalPath);
            index.Set(internalPath, entry);
         }
      }

      private void ResetDeleteHelper(string internalPath) {
         using (var index = indexProvider.Take()) {
            var entry = index.GetValueOrNull(internalPath);
            var blob = objectStore.Get(entry.RevisionHash);
            if (serializer.IsFileRevision(blob)) {
               File.Delete(GetAbsolutePath(internalPath));
            } else if (serializer.IsDirectoryRevision(blob)) {
               var directory = serializer.DeserializeDirectoryRevision(entry.RevisionHash, blob);
               foreach (var child in directory.Children) {
                  ResetDeleteHelper(BuildPath(internalPath, child.Key));
               }
               try {
                  Directory.Delete(GetAbsolutePath(internalPath), false);
               } catch (IOException e) {
                  /* eat exception - there are untracked files in the directory */
               }
            } else {
               throw new NotImplementedException("Internal Path did not point to File or Directory?");
            }
            index.Remove(internalPath);
         }
      }

      private void ResetFileHelper(string internalPath, FileRevision revision) 
      {
         using (var index = indexProvider.Take()) {
            var lastModified = GetTrueLastModified(internalPath);
            var entry = index.GetValueOrNull(internalPath);
            if (lastModified != entry.LastModified || revision.Hash != entry.RevisionHash) {
               File.WriteAllBytes(GetAbsolutePath(internalPath), revision.Data);
               entry.LastModified = GetTrueLastModified(internalPath);
               index.Set(internalPath, entry);
            }
         }
      }

      private DirectoryRevision GetDirectoryRevision(string internalPath, Hash160 hash)
      {
         var breadcrumbs = internalPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
         using (repositoryLock.Take())
         {
            var currentDirectory = serializer.DeserializeDirectoryRevision(hash, objectStore.Get(hash));
            for (var i = 0; i < breadcrumbs.Length; i++) {
               var nextHash = currentDirectory.Children[breadcrumbs[i]];
               currentDirectory = serializer.DeserializeDirectoryRevision(hash, objectStore.Get(nextHash));
            }
            return currentDirectory;
         }
      }

      private string GetAbsolutePath(string internalPath) { return Path.Combine(path, internalPath); }
      private ulong GetTrueLastModified(string internalPath) { return File.GetLastWriteTimeUtc(GetAbsolutePath(internalPath)).GetUnixTimeMilliseconds(); }
      private string BuildPath(params string[] strings) { return string.Join(PATH_DELIMITER, strings); }

      
      //      public IRevision GetRevisionOrNull(Hash160 hash) {
//         using (repositoryLock.Take()) {
//            objectStore.Get(hash);
//            throw new NotImplementedException();
//         }
//      }
//
//      public bool Reset(Hash160 hash) {
//         using (repositoryLock.Take()) {
//            var revision = GetRevisionOrNull(hash);
//            if (revision != null) {
//               //currentRevision = revision;
//               Reset();
//               return true;
//            } else {
//               return false;
//            }
//         }
//      }
//
//      private void Reset()
//      {
//         using (repositoryLock.Take()) {
//
//         }
//      }
//
//      public IReadOnlyDictionary<string, IBranch> EnumerateBranches() { return branches; }
//
//      public void Fetch(IRemoteRepository remote)
//      {
//      }

      private abstract class ReferenceCounter<TExposed> : IDisposable
      {
         private readonly object bigLock = new object();
         private int referenceCount = 0;

         public TExposed Take()
         {
            lock (bigLock) {
               if (referenceCount == 0) {
                  Initialize();
               }
               referenceCount++;
            }
            return GetExposed();
         }

         private void Return()
         {
            lock (bigLock) {
               referenceCount--;
               if (referenceCount == 0) {
                  Destroy();
               }
            }
         }

         protected abstract void Initialize();
         protected abstract void Destroy();
         protected abstract TExposed GetExposed();

         public void Dispose() { Return(); }
      }

      private class FileLock : ReferenceCounter<FileLock>
      {
         private readonly string path;
         private FileStream fileStream;

         public FileLock(string path) {
            this.path = path;
         }

         protected override void Initialize() { fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None); }

         protected override void Destroy() { if (fileStream != null) fileStream.Dispose(); }

         protected override FileLock GetExposed() { return this; }
      }

      private class ObjectStore
      {
         private readonly string path;

         public ObjectStore(string path) {
            this.path = path;
         }

         public byte[] Get(Hash160 hash) { 
            return File.ReadAllBytes(GetHashPath(hash));
         }

         public Hash160 Put(byte[] data)
         {
            using (var sha1 = new SHA1Managed()) {
               var hash = new Hash160(sha1.ComputeHash(data));
               var path = GetHashPath(hash);
               Util.PrepareParentDirectory(path);
               File.WriteAllBytes(path, data);
               return hash;
            }
         }

         private string GetHashPath(Hash160 hash)
         {
            var bucket = (hash.GetHashCode() * 13) % 256;
            var objectPath = Path.Combine(path, bucket.ToString(), hash.ToString("X"));
            return objectPath;
         }
      }

      private interface IIndex : IDisposable
      {
         IndexEntry GetValueOrNull(string internalPath);
         void Set(string internalPath, IndexEntry value);
         void Remove(string internalPath);
         IEnumerable<KeyValuePair<string, IndexEntry>> Enumerate();
      }

      private class IndexProvider : ReferenceCounter<IIndex>, IIndex
      {
         private readonly string path;
         private readonly IDictionary<string, IndexEntry> lastModifiedByInternalPath = new SortedDictionary<string, IndexEntry>();
         private bool dirty = false;

         public IndexProvider(string path) {
            this.path = path;
         }

         protected override void Initialize() { Load(); }

         protected override void Destroy()
         {
            if (dirty) {
               Save();
            }
         }

         protected override IIndex GetExposed() { return this; }

         private void Load()
         {
            lastModifiedByInternalPath.Clear();

            if (File.Exists(path)) {
               var data = File.ReadAllBytes(path);
               using (var ms = new MemoryStream(data))
               using (var reader = new BinaryReader(ms)) {
                  var count = reader.ReadUInt32();
                  for (uint i = 0; i < count; i++) {
                     var internalPath = reader.ReadNullTerminatedString();
                     var modified = reader.ReadIndexEntry();
                     lastModifiedByInternalPath.Add(internalPath, modified);
                  }
               }
               dirty = false;
            } else {
               dirty = true;
            }
         }

         private void Save() {
            Util.PrepareParentDirectory(path);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) 
            using (var writer = new BinaryWriter(fs)) {
               writer.Write((uint)lastModifiedByInternalPath.Count);
               foreach (var kvp in lastModifiedByInternalPath) {
                  writer.WriteNullTerminatedString(kvp.Key);
                  writer.Write((IndexEntry)kvp.Value);
               }
            }
             
            dirty = false;
         }

         IndexEntry IIndex.GetValueOrNull(string internalPath)
         {
            IndexEntry result;
            if (!lastModifiedByInternalPath.TryGetValue(internalPath, out result))
               return new IndexEntry(0, Hash160.Zero, 0);
            return result;
         }

         void IIndex.Set(string internalPath, IndexEntry value)
         {
            lastModifiedByInternalPath[internalPath] = value;
            dirty = true;
         }

         void IIndex.Remove(string internalPath)
         {
            lastModifiedByInternalPath.Remove(internalPath);
            dirty = true;
         }

         IEnumerable<KeyValuePair<string, IndexEntry>> IIndex.Enumerate() { return lastModifiedByInternalPath; } 
      }

      public class DpmSerializer
      {
         private const uint DIRECTORY_REVISION_MAGIC = 0x444D5044U; // "DPMD"
         private const uint FILE_REVISION_MAGIC = 0x464D5044U; // "DPMF"

         public DpmSerializer() { }

         public DirectoryRevision DeserializeDirectoryRevision(Hash160 directoryHash, byte[] data)
         {
            using (var ms = new MemoryStream(data)) 
            using (var reader = new BinaryReader(ms)) {
               var magic = reader.ReadUInt32();
               if (magic != DIRECTORY_REVISION_MAGIC) {
                  throw new InvalidOperationException("DPMD Magic Mismatch - expected " + DIRECTORY_REVISION_MAGIC + " but found " + magic);
               }
               var childCount = reader.ReadUInt32();
               var childrenDictionary = new ListDictionary<string, Hash160>();
               for (uint i = 0; i < childCount; i++) {
                  var name = reader.ReadNullTerminatedString();
                  var hash = reader.ReadHash160();
                  childrenDictionary.Add(name, hash);
               }
               return new DirectoryRevision(directoryHash, childrenDictionary);
            }
         }

         public byte[] SerializeDirectoryRevision(DirectoryRevision directory) 
         {
            using (var ms = new MemoryStream()) {
               using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
                  writer.Write(DIRECTORY_REVISION_MAGIC);
                  writer.Write((uint)directory.Children.Count);
                  foreach (var child in directory.Children) {
                     writer.WriteNullTerminatedString(child.Key);
                     writer.Write(child.Value);
                  }
               }
               return ms.ToArray();
            }
         }

         public FileRevision DeserializeFileRevision(Hash160 fileHash, byte[] data) {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms)) {
               var magic = reader.ReadUInt32();
               if (magic != FILE_REVISION_MAGIC) {
                  throw new InvalidOperationException("DPMF Magic Mismatch - Expected " + FILE_REVISION_MAGIC + " but found " + magic);
               }

               var bodyLength = reader.ReadUInt32();
               var body = reader.ReadBytes((int)bodyLength);
               var deflateStream = new DeflateStream(new MemoryStream(body), CompressionMode.Decompress);
               var decompressedStream = new MemoryStream();
               deflateStream.CopyTo(decompressedStream);
               return new FileRevision(fileHash, decompressedStream.ToArray());
            }
         }

         public bool IsFileRevision(byte[] blob) { return blob.Length > 4 && BitConverter.ToUInt32(blob, 0) == FILE_REVISION_MAGIC; }
         public bool IsDirectoryRevision(byte[] blob) { return blob.Length > 4 && BitConverter.ToUInt32(blob, 0) == DIRECTORY_REVISION_MAGIC; }
      }

      public class DirectoryRevision
      {
         private Hash160 hash;
         private IDictionary<string, Hash160> children;

         public DirectoryRevision(Hash160 hash, IDictionary<string, Hash160> children)
         {
            this.hash = hash;
            this.children = children;
         }

         public Hash160 Hash { get { return hash; } }
         public IDictionary<string, Hash160> Children { get { return children; } } 
      }

      public class FileRevision
      {
         private Hash160 hash;
         private byte[] data;

         public FileRevision(Hash160 hash, byte[] data)
         {
            this.hash = hash;
            this.data = data;
         }

         public Hash160 Hash { get { return hash; } }
         public byte[] Data { get { return data; } }
      }
   }


   public class IndexEntry
   {
      private ulong lastModified;
      private Hash160 revisionHash;
      private IndexEntryFlags indexEntryFlags;

      public IndexEntry(ulong lastModified, Hash160 revisionHash, IndexEntryFlags indexEntryFlags)
      {
         this.lastModified = lastModified;
         this.revisionHash = revisionHash;
         this.indexEntryFlags = indexEntryFlags;
      }

      public ulong LastModified { get { return lastModified; } set { lastModified = value; } }
      public Hash160 RevisionHash { get { return revisionHash; } set { revisionHash = value; } }
      public IndexEntryFlags Flags { get { return indexEntryFlags; } set { indexEntryFlags = value; } }
   }

   [Flags]
   public enum IndexEntryFlags : uint
   {
      Added       = 0x00000001U,
      Removed     = 0x00000002U,
      Directory   = 0x01000000U
   }

   public static class Extensions
   {
      public static Hash160 ReadHash160(this BinaryReader reader) { return new Hash160(reader.ReadBytes(Hash160.Size)); }
      public static void Write(this BinaryWriter writer, Hash160 hash) { writer.Write(hash.GetBytes()); }

      public static IndexEntry ReadIndexEntry(this BinaryReader reader) { return new IndexEntry(reader.ReadUInt64(), reader.ReadHash160(), (IndexEntryFlags)reader.ReadUInt32()); }
      public static void Write(this BinaryWriter writer, IndexEntry entry) { writer.Write(entry.LastModified); writer.Write(entry.RevisionHash); writer.Write((UInt32)entry.Flags); }
   }

   public interface IBranch
   {
      string Name { get; }
   }

   public interface IRevision { }
   public interface IRemoteRepository { }
   public interface IPackage { }
}
