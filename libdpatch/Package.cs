using ItzWarty;
using ItzWarty.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dargon.Patcher
{
   public interface IRepository
   {
      IRevision GetRevisionOrNull(Guid guid);
   }

   [Flags]
   public enum ChangeType : byte
   {
      Staged = 0x10,
      Unstaged = 0x20,

      Added = 0x01,
      Removed = 0x02,
      Modified = 0x04
   }

   public class LocalRepository
   {
      private const string PATH_DELIMITER = "/";
      private const string DEFAULT_BRANCH = "master";

      private readonly string root;
      private readonly string dpmPath;
      private readonly ObjectStore objectStore;
      private readonly FileLock repositoryLock;
      private readonly IndexProvider indexProvider;
      private readonly ReferenceManager referenceManager;
      private readonly StateManager stateManager;
      private readonly ConfigurationManager configurationManager;
      private readonly DpmSerializer serializer = new DpmSerializer();

      public LocalRepository(string root)
      {
         this.root = root;
         this.dpmPath = Path.Combine(root, ".dpm");
         Util.PrepareDirectory(dpmPath);
         this.objectStore = new ObjectStore(Path.Combine(dpmPath, "objects"));
         this.repositoryLock = new FileLock(Path.Combine(dpmPath, "LOCK"));
         this.referenceManager = new ReferenceManager(Path.Combine(dpmPath, "refs"));
         this.stateManager = new StateManager(dpmPath);
         this.configurationManager = new ConfigurationManager(dpmPath);
         this.indexProvider = new IndexProvider(Path.Combine(dpmPath, "LAST_MODIFIED"));
      }

      public string Root { get { return root; } }
      public HeadDescriptor Head { get { using (repositoryLock.Take()) return stateManager.GetHead(); } }

      public bool GetIsInitialized()
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            return index.GetValueOrNull("") != null;
         }
      }

      public void Initialize()
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            configurationManager.Identity = Environment.UserName;

            var dir = new DirectoryRevision(Hash160.Zero, new Dictionary<string, Hash160>());
            var hash = objectStore.Put(serializer.SerializeDirectoryRevision(dir));
            index.Set("", new IndexEntry(GetTrueLastModifiedInternal(""), hash, IndexEntryFlags.Directory));
            referenceManager.CreateHead(DEFAULT_BRANCH);
            referenceManager.SetHeadCommitHash(DEFAULT_BRANCH, Hash160.Zero);
            stateManager.SetHead(DEFAULT_BRANCH);
         }
      }

      public void AddFile(string internalPath)
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var entry = index.GetValueOrNull(internalPath);
            if (entry == null) {
               var fileRevision = new FileRevision(Hash160.Zero, File.ReadAllBytes(GetAbsolutePath(internalPath)));
               Console.WriteLine("FILE REVISION HAS DATA LENGTH " + fileRevision.Data.Length);
               var hash = objectStore.Put(serializer.SerializeFileRevision(fileRevision));
               index.Set(internalPath, new IndexEntry(GetTrueLastModifiedInternal(internalPath), hash, IndexEntryFlags.Added));
            }
         }
      }

      public void UpdateFile(string internalPath)
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var entry = index.GetValueOrNull(internalPath);
            if (entry != null) {
               entry.Flags |= IndexEntryFlags.Modified;
               entry.LastModified = GetTrueLastModifiedInternal(internalPath);
               entry.RevisionHash = objectStore.Put(serializer.SerializeFileRevision(new FileRevision(Hash160.Zero, File.ReadAllBytes(GetAbsolutePath(internalPath)))));
               index.Set(internalPath, entry);
            }
         }
      }

      public void Remove(string internalPath, bool deletePhysically = false)
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

      public Hash160 Commit(string message)
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var childrenByParentPathLengthDescending = new MultiValueSortedDictionary<string, Tuple<string, ChangeType, IndexEntry>>(new LambdaComparer<string>((a, b) => -a.Length.CompareTo(b.Length)));
            foreach (var entry in EnumerateChangedFiles()) {
               if (entry.Value.HasFlag(ChangeType.Staged)) {
                  var breadcrumbs = entry.Key.Split('/');
                  var parentPath = breadcrumbs.SubArray(0, breadcrumbs.Length - 1).Join(PATH_DELIMITER);
                  var name = breadcrumbs[breadcrumbs.Length - 1];
                  childrenByParentPathLengthDescending.Add(parentPath, new Tuple<string, ChangeType, IndexEntry>(name, entry.Value, index.GetValueOrNull(entry.Key)));
               }
            }
            while (childrenByParentPathLengthDescending.Any()) {
               var directoryAndChildren = childrenByParentPathLengthDescending.First();
               childrenByParentPathLengthDescending.Remove(directoryAndChildren.Key);

               var directoryPath = directoryAndChildren.Key;
               var directoryEntry = index.GetValueOrNull(directoryPath);
               var directory = serializer.DeserializeDirectoryRevision(directoryEntry.RevisionHash, objectStore.Get(directoryEntry.RevisionHash));
               var childNamesAndEntries = directoryAndChildren.Value;
               foreach (var childNameAndEntry in childNamesAndEntries) {
                  var name = childNameAndEntry.Item1;
                  var changeType = childNameAndEntry.Item2;
                  var entry = childNameAndEntry.Item3;
                  var entryInternalPath = BuildPath(directoryPath, name);
                  Console.WriteLine("ENTRY INTERNAL PATH " + entryInternalPath + " directory is " + directory + " and entry is named " + name);
                  var keepInIndex = true;
                  if (entry.Flags.HasFlag(IndexEntryFlags.Added)) {
                     entry.Flags &= ~IndexEntryFlags.Added;
                     directory.Children.Add(name, entry.RevisionHash);
                  } else if (entry.Flags.HasFlag(IndexEntryFlags.Removed)) {
                     entry.Flags &= ~IndexEntryFlags.Removed;
                     directory.Children.Remove(name);
                     keepInIndex = false;
                  } else if (entry.Flags.HasFlag(IndexEntryFlags.Modified)){
                     if (entry.Flags.HasFlag(IndexEntryFlags.Directory)) {
                        // directory update handled below - don't have to update entry revision hash as that's already been done
                     } else {
                        entry.RevisionHash = objectStore.Put(File.ReadAllBytes(GetAbsolutePath(entryInternalPath)));
                        entry.Flags &= ~IndexEntryFlags.Modified;
                     }
                     directory.Children[name] = entry.RevisionHash;
                  }
                  entry.LastModified = GetTrueLastModifiedInternal(entryInternalPath);
                  if (keepInIndex) {
                     index.Set(entryInternalPath, entry);
                  } else {
                     index.Remove(entryInternalPath);
                  }
               }
               directoryEntry.RevisionHash = objectStore.Put(serializer.SerializeDirectoryRevision(directory));
               directoryEntry.LastModified = GetTrueLastModifiedInternal(directoryPath);
               index.Set(directoryPath, directoryEntry);
            }

            var headCommit = GetHeadCommitHash(Head);

            // create new commit object which points to new root object and has parent reference to old commit
            Console.WriteLine("Adding commit object...");
            var commitObject = new CommitObject(headCommit, new Hash160[0], GetRootHash(), configurationManager.Identity, message, DateTime.UtcNow.GetUnixTimeMilliseconds());
            var commitObjectHash = objectStore.Put(serializer.SerializeCommitObject(commitObject));
            Console.WriteLine("Commit object has hash " + commitObjectHash.ToString("x"));

            // move head forward
            var head = Head;
            if (head.Type == HeadType.Branch) {
               Console.WriteLine("Advancing branch " + head.Value + " to " + commitObjectHash.ToString("x"));
               referenceManager.SetHeadCommitHash(head.Value, commitObjectHash);
            } else {
               stateManager.SetHead(commitObjectHash);
            }

            return commitObjectHash;
         }
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

      public List<KeyValuePair<string, ChangeType>> EnumerateChangedFiles()
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            return EnumerateChangedFiles("", index.GetValueOrNull(""));
         }
      }

      private List<KeyValuePair<string, ChangeType>> EnumerateChangedFiles(string internalPath, IndexEntry comparedEntry)
      {
         using (repositoryLock.Take())
         using (var index = indexProvider.Take()) {
            var result = new List<KeyValuePair<string, ChangeType>>();
            var realDirectoryPath = GetAbsolutePath(internalPath);
            var directoryRevision = serializer.DeserializeDirectoryRevision(comparedEntry.RevisionHash, objectStore.Get(comparedEntry.RevisionHash));
            var realEntries = Directory.EnumerateFileSystemEntries(realDirectoryPath).Where(path => new FileInfo(path).Name != ".dpm").ToDictionary(path => new FileInfo(path).Name, path => new { Path = path }); 
            var indexEntries = directoryRevision.Children.ToDictionary((kvp) => kvp.Key, kvp => new { Path = BuildPath(internalPath, kvp.Key), Hash = kvp.Value });
            var realFileNames = new HashSet<string>(realEntries.Keys); 
            var indexFileNames = new HashSet<string>(indexEntries.Keys);
            var sharedNames = new HashSet<string>(realFileNames).With(set => set.IntersectWith(indexFileNames));
            var realOnlyNames = new HashSet<string>(realFileNames).With(set => set.ExceptWith(sharedNames));
            var indexOnlyNames = new HashSet<string>(indexFileNames).With(set => set.ExceptWith(sharedNames));
            foreach (var fileName in realOnlyNames) {
               var fileInternalPath = BuildPath(internalPath, fileName);
               var fileInfo = new FileInfo(GetAbsolutePath(fileInternalPath));
               if (fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
                  throw new NotImplementedException("Add all children of fileInternalPath (Only real entry exists)");
               } else {
                  var addedFilePath = BuildPath(internalPath, fileName);
                  bool staged = index.GetValueOrNull(addedFilePath) != null;
                  result.Add(new KeyValuePair<string, ChangeType>(addedFilePath, ChangeType.Added | (staged ? ChangeType.Staged : ChangeType.Unstaged)));
               }
            }
            foreach (var fileName in indexOnlyNames) {
               var fileInternalPath = BuildPath(internalPath, fileName);
               var entry = indexEntries[fileName];
               if (serializer.IsDirectoryRevision(objectStore.Get(entry.Hash))) {
                  throw new NotImplementedException("Add all children of fileInternalPath (Only index entry exists)");
               } else {
                  result.Add(new KeyValuePair<string, ChangeType>(BuildPath(internalPath, fileName), ChangeType.Removed | ChangeType.Unstaged));
               }
            }
            foreach (var fileName in sharedNames) {
               var realEntry = realEntries[fileName];
               var indexEntry = index.GetValueOrNull(indexEntries[fileName].Path);
               if (indexEntry.Flags.HasFlag(IndexEntryFlags.Added)) {
                  result.Add(new KeyValuePair<string, ChangeType>(fileName, ChangeType.Added | ChangeType.Staged));
               } else if (indexEntry.Flags.HasFlag(IndexEntryFlags.Added)) {
                  result.Add(new KeyValuePair<string, ChangeType>(fileName, ChangeType.Removed | ChangeType.Staged));
               } else if (indexEntry.Flags.HasFlag(IndexEntryFlags.Modified)) {
                  result.Add(new KeyValuePair<string, ChangeType>(fileName, ChangeType.Modified | ChangeType.Staged));
               } else {
                  if (GetTrueLastModified(realEntry.Path) != indexEntry.LastModified) {
                     var fileRevision = serializer.DeserializeFileRevision(indexEntry.RevisionHash, objectStore.Get(indexEntry.RevisionHash));
                     var fileRevisionData = fileRevision.Data;
                     if (fileRevisionData.Length != new FileInfo(realEntry.Path).Length) {
                        result.Add(new KeyValuePair<string, ChangeType>(fileName, ChangeType.Modified | ChangeType.Unstaged));
                     } else {
                        var diskContent = File.ReadAllBytes(realEntry.Path);
                        bool equal = Util.ByteArraysEqual(diskContent, fileRevisionData);
                        if (equal) {
                           indexEntry.LastModified = GetTrueLastModified(realEntry.Path);
                           index.Set(indexEntries[fileName].Path, indexEntry);
                        } else {
                           result.Add(new KeyValuePair<string, ChangeType>(fileName, ChangeType.Modified | ChangeType.Unstaged));
                        }
                     }
                  }
               }
            }
            return result;
         }
      }

      //            var entries = index.Enumerate();
      //            var changedEntries = new List<Tuple<string, IndexEntry, ChangeType>>(); 
      //            foreach (var entry in entries) {
      //               if (entry.Value.Flags.HasFlag(IndexEntryFlags.Directory)) {
      //                  continue;
      //               }
      //
      //               if (entry.Value.Flags.HasFlag(IndexEntryFlags.Added)) {
      //                  var changeType = ChangeType.Added;
      //                  if (entry.Value.Flags.HasFlag(IndexEntryFlags.Added)) {
      //                     changeType |= ChangeType.Staged;
      //                  } else {
      //                     changeType |= ChangeType.Staged;
      //                  }
      //                  changedEntries.Add(new Tuple<string, IndexEntry, ChangeType>(entry.Key, entry.Value, ));
      //               } else if (GetTrueLastModifiedInternal(entry.Key) != entry.Value.LastModified) {
      //                  var fileRevision = serializer.DeserializeFileRevision(entry.Value.RevisionHash, objectStore.Get(entry.Value.RevisionHash));
      //                  var fileRevisionData = fileRevision.Data;
      //                  if (fileRevisionData.Length != new FileInfo(GetAbsolutePath(entry.Key)).Length) {
      //                     changedEntries.Add(entry);
      //                  } else {
      //                     var diskContent = File.ReadAllBytes(GetAbsolutePath(entry.Key));
      //                     bool equal = true;
      //                     for (var i = 0; i < diskContent.Length && equal; i++) {
      //                        equal &= fileRevisionData[i] == diskContent[i];
      //                     }
      //                     if (equal) {
      //                        entry.Value.LastModified = GetTrueLastModifiedInternal(entry.Key);
      //                        index.Set(entry.Key, entry.Value);
      //                     } else {
      //                        changedEntries.Add(entry);
      //                     }
      //                  }
      //               }
      //            }
      //            return changedEntries;

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
            var lastModified = GetTrueLastModifiedInternal(internalPath);
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
            entry.LastModified = GetTrueLastModifiedInternal(internalPath);
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
               } catch (IOException) {
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
            var lastModified = GetTrueLastModifiedInternal(internalPath);
            var entry = index.GetValueOrNull(internalPath);
            if (lastModified != entry.LastModified || revision.Hash != entry.RevisionHash) {
               File.WriteAllBytes(GetAbsolutePath(internalPath), revision.Data);
               entry.LastModified = GetTrueLastModifiedInternal(internalPath);
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

      public IEnumerable<Hash160> EnumerateObjectStoreKeys() { return objectStore.EnumerateKeys(); }

      public IEnumerable<KeyValuePair<string, IndexEntry>> EnumerateIndexEntries()
      {
         using (var index = indexProvider.Take()) {
            return index.Enumerate();
         }
      }

      public Hash160 GetRootHash()
      {
         using (var index = indexProvider.Take()) {
            return index.GetValueOrNull("").RevisionHash;
         }
      }

      public Hash160 GetBranchCommitHash(string branch) { using (repositoryLock.Take()) return referenceManager.GetHeadCommitHash(branch); }

      public Hash160 GetCommitRootHash(Hash160 commitHash)
      {
         if (commitHash.Equals(Hash160.Zero)) {
            return Hash160.Zero;
         }

         using (repositoryLock.Take()) 
            return serializer.DeserializeCommitObject(commitHash, objectStore.Get(commitHash)).RootRevisionHash;
      }

      public Hash160 GetHeadCommitHash(HeadDescriptor head)
      {
         using (repositoryLock.Take()) {
            if (head.Type == HeadType.Branch) {
               return referenceManager.GetHeadCommitHash(head.Value);
            } else if (head.Type == HeadType.Commit) {
               return Hash160.Parse(head.Value);
            } else {
               throw new NotImplementedException("Unhandled head type " + head.Type);
            }
         }
      }

      private string SanitizeInternalPath(string path) { return path.Trim(new[] { '/', '\\' }); }
      private string GetAbsolutePath(string internalPath) { return Path.Combine(root, internalPath); }
      private ulong GetTrueLastModified(string realPath) { return File.GetLastWriteTimeUtc(realPath).GetUnixTimeMilliseconds(); }
      private ulong GetTrueLastModifiedInternal(string internalPath) { return GetTrueLastModified(GetAbsolutePath(internalPath)); }
      private string BuildPath(params string[] strings) { return SanitizeInternalPath(string.Join(PATH_DELIMITER, strings)); }

      private class OtherBranch
      {
         
      }

      public void PrintLinearGraph()
      {
         var head = stateManager.GetHead();
         var commitHash = GetHeadCommitHash(head);
         while (commitHash != Hash160.Zero) {
            var commit = serializer.DeserializeCommitObject(commitHash, objectStore.Get(commitHash));
            Console.WriteLine("* " + commitHash.ToString("x").Substring(0, 8) + " - " + commit.Message + " - " + commit.Author);
            commitHash = commit.ParentHash;
         }
      }

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
         private readonly bool kDebugEnabled = false;
         private readonly string path;

         public ObjectStore(string path) {
            this.path = path;
            Util.PrepareDirectory(path);
         }

         public byte[] Get(Hash160 hash)
         {
            if (kDebugEnabled) Console.WriteLine("Getting data of hash " + hash.ToString("X") + " from path " + GetHashPath(hash));
            return File.ReadAllBytes(GetHashPath(hash));
         }

         public Hash160 Put(byte[] data)
         {
            using (var sha1 = new SHA1Managed()) {
               var hash = new Hash160(sha1.ComputeHash(data));
               var path = GetHashPath(hash);
               if (kDebugEnabled) Console.WriteLine("Putting " + data.Length + " bytes of hash " + hash.ToString("X") + " to path " + path);
               Util.PrepareParentDirectory(path);
               File.WriteAllBytes(path, data);
               return hash;
            }
         }

         public IEnumerable<Hash160> EnumerateKeys()
         {
            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
            return files.Select(file => new FileInfo(file).Name).Where(name => name.Length == Hash160.Size * 2).Select(Hash160.Parse);
         } 

         private string GetHashPath(Hash160 hash)
         {
            var bucket = (unchecked((uint)hash.GetHashCode()) * 13) % 256;
            var objectPath = Path.Combine(path, bucket.ToString("x"), hash.ToString("x"));
            return objectPath;
         }
      }

      public class ConfigurationManager
      {
         private const string IDENTITY_KEY = "identity";
         private readonly string dpmLocalPath;

         public ConfigurationManager(string dpmLocalPath) {
            this.dpmLocalPath = dpmLocalPath;
         }

         public string Identity { get { return this[IDENTITY_KEY]; } set { this[IDENTITY_KEY] = value; } }
         private string this[string key] { get { return File.ReadAllText(Path.Combine(dpmLocalPath, key), Encoding.UTF8); } set { File.WriteAllText(Path.Combine(dpmLocalPath, key), value, Encoding.UTF8); } }
      }

      public class StateManager
      {
         private const string kHeadTypeBranch = "Branch";
         private const string kHeadTypeCommit = "Commit";

         private readonly string path;
         private readonly string headPath;

         public StateManager(string path) {
            this.path = path;
            this.headPath = Path.Combine(path, "head");
         }

         public HeadDescriptor GetHead()
         {
            var body = File.ReadAllText(headPath, Encoding.UTF8);
            var delimiterIndex = body.IndexOf(':');
            var typeString = body.Substring(0, delimiterIndex).Trim();
            var valueString = body.Substring(delimiterIndex + 1).Trim();
            HeadType type = 0;
            if (typeString.Equals(kHeadTypeBranch, StringComparison.OrdinalIgnoreCase)) {
               type = HeadType.Branch;
            } else if (typeString.Equals(kHeadTypeCommit, StringComparison.OrdinalIgnoreCase)) {
               type = HeadType.Commit;
            }
            return new HeadDescriptor(type, valueString);
         }

         public void SetHead(Hash160 commitHash) { SetHeadInternal(kHeadTypeCommit + ": " + commitHash.ToString("x")); }
         public void SetHead(string branch) { SetHeadInternal(kHeadTypeBranch + ": " + branch); }
         public void SetHeadInternal(string value) { File.WriteAllText(headPath, value); }
      }

      public enum HeadType
      {
         Branch,
         Commit
      }

      public class HeadDescriptor
      {
         private HeadType type;
         private string value;

         public HeadDescriptor(HeadType type, string value)
         {
            this.type = type;
            this.value = value;
         }

         public HeadType Type { get { return type; } }
         public string Value { get { return value; } }
      }

      public class ReferenceManager
      {
         private readonly string path;
         private readonly string headsPath;

         public ReferenceManager(string path) {
            this.path = path;
            this.headsPath = Path.Combine(path, "heads");
            Util.PrepareDirectory(headsPath);
         }

         public IReadOnlyDictionary<string, Hash160> EnumerateHeads() { return EnumerateReferences(headsPath); }

         public void CreateHead(string head) { CreateReference(headsPath, head, Hash160.Zero); }

         public void SetHeadCommitHash(string head, Hash160 hash) { SetReferenceCommitHash(headsPath, head, hash); }

         public Hash160 GetHeadCommitHash(string head) { return GetReferenceCommitHash(headsPath, head); }

         public void DeleteHead(string head) { DeleteReference(headsPath, head); }

         private IReadOnlyDictionary<string, Hash160> EnumerateReferences(string parentPath)
         {
            var result = new ListDictionary<string, Hash160>();
            foreach (var filePath in Directory.EnumerateFiles(parentPath)) {
               var info = new FileInfo(filePath);
               result.Add(info.Name, GetReferenceCommitHash(parentPath, info.Name));
            }
            return result;
         }

         public void CreateReference(string parentPath, string referenceName, Hash160 commitHash) { SetReferenceCommitHash(parentPath, referenceName, commitHash); }

         private void SetReferenceCommitHash(string parentPath, string referenceName, Hash160 commitHash) { File.WriteAllText(Path.Combine(parentPath, referenceName), commitHash.ToString("x"), Encoding.ASCII); }

         public Hash160 GetReferenceCommitHash(string parentPath, string referenceName) { return Hash160.Parse(File.ReadAllText(Path.Combine(parentPath, referenceName), Encoding.ASCII).Trim()); }

         public void DeleteReference(string parentPath, string referenceName) { File.Delete(Path.Combine(parentPath, referenceName)); }
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
         private readonly IDictionary<string, IndexEntry> entriesByInternalPath = new SortedDictionary<string, IndexEntry>();
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
            entriesByInternalPath.Clear();

            if (File.Exists(path)) {
               var data = File.ReadAllBytes(path);
               using (var ms = new MemoryStream(data))
               using (var reader = new BinaryReader(ms)) {
                  var count = reader.ReadUInt32();
                  for (uint i = 0; i < count; i++) {
                     var internalPath = reader.ReadNullTerminatedString();
                     var modified = reader.ReadIndexEntry();
                     entriesByInternalPath.Add(internalPath, modified);
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
               writer.Write((uint)entriesByInternalPath.Count);
               foreach (var kvp in entriesByInternalPath) {
                  writer.WriteNullTerminatedString(kvp.Key);
                  writer.Write((IndexEntry)kvp.Value);
               }
            }
             
            dirty = false;
         }

         IndexEntry IIndex.GetValueOrNull(string internalPath)
         {
            IndexEntry result;
            if (!entriesByInternalPath.TryGetValue(internalPath, out result))
               return null;
            return result;
         }

         void IIndex.Set(string internalPath, IndexEntry value)
         {
            entriesByInternalPath[internalPath] = value;
            dirty = true;
         }

         void IIndex.Remove(string internalPath)
         {
            entriesByInternalPath.Remove(internalPath);
            dirty = true;
         }

         IEnumerable<KeyValuePair<string, IndexEntry>> IIndex.Enumerate() { return entriesByInternalPath; } 
      }

      public class DpmSerializer
      {
         private const uint DIRECTORY_REVISION_MAGIC = 0x444D5044U; // "DPMD"
         private const uint FILE_REVISION_MAGIC = 0x464D5044U; // "DPMF"
         private const uint COMMIT_OBJECT_MAGIC = 0x434D5044U; // "DPMC"

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

         public byte[] SerializeFileRevision(FileRevision fileRevision)
         {
            using (var ms = new MemoryStream()) {
               using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
                  writer.Write(FILE_REVISION_MAGIC);
                  var compressedStream = new MemoryStream();
                  using (var compressionStream = new DeflateStream(compressedStream, CompressionLevel.Optimal))
                  using (var dataStream = new MemoryStream(fileRevision.Data)) {
                     dataStream.CopyTo(compressionStream);
                  }
                  var data = compressedStream.ToArray();
                  writer.Write((uint)data.Length);
                  writer.Write(data);
               }
               return ms.ToArray();
            }
         }

         public CommitObject DeserializeCommitObject(Hash160 commitHash, byte[] data)
         {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms)) {
               var magic = reader.ReadUInt32();
               if (magic != COMMIT_OBJECT_MAGIC) {
                  throw new InvalidOperationException("DPMC Magic Mismatch - Expected " + COMMIT_OBJECT_MAGIC + " but found " + magic);
               }

               var parentCommitHash = reader.ReadHash160();
               var fakeParentCommitHashes = Util.Generate((int)reader.ReadUInt32(), (i) => reader.ReadHash160());
               var rootDirectoryHash = reader.ReadHash160();
               var author = reader.ReadNullTerminatedString();
               var message = reader.ReadNullTerminatedString();
               var dateCreated = reader.ReadUInt64();
               return new CommitObject(parentCommitHash, fakeParentCommitHashes, rootDirectoryHash, author, message, dateCreated);
            }
         }

         public byte[] SerializeCommitObject(CommitObject commitObject)
         {
            using (var ms = new MemoryStream()) {
               using (var writer = new BinaryWriter(ms, Encoding.UTF8, true)) {
                  writer.Write(COMMIT_OBJECT_MAGIC);
                  writer.Write(commitObject.ParentHash);
                  writer.Write((uint)commitObject.FakeParentHashes.Length);
                  foreach (var hash in commitObject.FakeParentHashes) {
                     writer.Write(hash);
                  }
                  writer.Write(commitObject.RootRevisionHash);
                  writer.WriteNullTerminatedString(commitObject.Author);
                  writer.WriteNullTerminatedString(commitObject.Message);
                  writer.Write(commitObject.DateCreated);
               }
               return ms.ToArray();
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
         private readonly Hash160 hash;
         private readonly byte[] data;

         public FileRevision(Hash160 hash, byte[] data)
         {
            this.hash = hash;
            this.data = data;
         }

         public Hash160 Hash { get { return hash; } }
         public byte[] Data { get { return data; } }
      }

      public class CommitObject
      {
         private readonly Hash160 parentHash;
         private readonly Hash160[] fakeParentHashes;
         private readonly Hash160 rootRevisionHash;
         private readonly string author;
         private readonly string message;
         private readonly ulong dateCreated;

         public CommitObject(Hash160 parentHash, Hash160[] fakeParentHashes, Hash160 rootRevisionHash, string author, string message, ulong dateCreated)
         {
            this.parentHash = parentHash;
            this.fakeParentHashes = fakeParentHashes;
            this.rootRevisionHash = rootRevisionHash;
            this.author = author;
            this.message = message;
            this.dateCreated = dateCreated;
         }

         public Hash160 ParentHash { get { return parentHash; } }
         public Hash160[] FakeParentHashes { get { return fakeParentHashes; } }
         public Hash160 RootRevisionHash { get { return rootRevisionHash; } }
         public string Message { get { return message; } }
         public string Author { get { return author; } }
         public ulong DateCreated { get { return dateCreated; } }
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
      Modified    = 0x00000004U,
      Directory   = 0x01000000U
   }

   public interface IBranch
   {
      string Name { get; }
   }

   public interface IRevision { }
   public interface IRemoteRepository { }
   public interface IPackage { }
}
