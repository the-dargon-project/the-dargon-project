using System;
using System.IO;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon {
   public class ClientSystemStateImpl : SystemState {
      private const string kSystemStateConfigurationDirectoryName = "system-state";
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly string configurationDirectoryPath;
      private readonly string basePath;

      public ClientSystemStateImpl(IFileSystemProxy fileSystemProxy, string configurationDirectoryPath) {
         this.fileSystemProxy = fileSystemProxy;
         this.basePath = Path.Combine(configurationDirectoryPath, kSystemStateConfigurationDirectoryName);
         fileSystemProxy.PrepareDirectory(basePath);
      }

      public string Get(string key, string defaultValue) {
         string result;
         if (!TryGet(key, out result)) {
            result = defaultValue;
         }
         return result;
      }

      public bool TryGet(string key, out string value) {
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

      public void Set(string key, string value) {
         var path = BuildKeyPath(key);
         fileSystemProxy.PrepareParentDirectory(path);
         fileSystemProxy.WriteAllText(path, value);
      }
      
      public bool Get(string key, bool defaultValue) {
         bool result;
         if (!TryGet(key, out result)) {
            result = defaultValue;
         }
         return result;
      }

      public bool TryGet(string key, out bool value) {
         string valueString;
         if (TryGet(key, out valueString) &&
             bool.TryParse(valueString, out value)) {
            return true;
         } else {
            value = false;
            return false;
         }
      }

      public void Set(string key, bool value) {
         Set(key, value.ToString());
      }

      public int Get(string key, int defaultValue) {
         int result;
         if (!TryGet(key, out result)) {
            result = defaultValue;
         }
         return result;
      }

      public bool TryGet(string key, out int value) {
         string valueString;
         if (TryGet(key, out valueString) &&
             int.TryParse(valueString, out value)) {
            return true;
         } else {
            value = 0;
            return false;
         }
      }

      public void Set(string key, int value) {
         Set(key, value.ToString());
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