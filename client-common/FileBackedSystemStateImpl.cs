using System;
using System.IO;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon {
   public class FileBackedSystemStateImpl : SystemStateImplBase {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly string basePath;

      public FileBackedSystemStateImpl(IFileSystemProxy fileSystemProxy, string basePath) {
         this.fileSystemProxy = fileSystemProxy;
         this.basePath = basePath;
         fileSystemProxy.PrepareDirectory(basePath);
      }

      public override bool TryGet(string key, out string value) {
         var path = BuildKeyPath(key);
         var fileInfo = fileSystemProxy.GetFileSystemInfo(path);
         if (fileInfo.Exists && !fileInfo.Attributes.HasFlag(FileAttributes.Directory)) {
            value = fileSystemProxy.ReadAllText(path);
            return true;
         } else {
            value = null;
            return false;
         }
      }

      public override void Set(string key, string value) {
         var path = BuildKeyPath(key);
         fileSystemProxy.PrepareParentDirectory(path);
         fileSystemProxy.WriteAllText(path, value);
      }

      private string BuildKeyPath(string key) {
         var parts = key.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
         var path = basePath;
         foreach (var part in parts) {
            path = Path.Combine(path, part);
         }
         return path;
      }
   }
}