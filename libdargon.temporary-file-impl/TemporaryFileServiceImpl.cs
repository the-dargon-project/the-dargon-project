using ItzWarty;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Dargon {
   public class TemporaryFileServiceImpl : TemporaryFileService
   {
      private const string EXPIRATION_FILE_NAME = ".expires";
      private readonly IClientConfiguration configuration;
      private readonly string temporaryDirectoryPath;
      private readonly string temporaryFilesLockPath;
      private readonly FileLock temporaryFilesLock;

      public TemporaryFileServiceImpl(IClientConfiguration configuration) {
         this.configuration = configuration;
         this.temporaryDirectoryPath = Path.Combine(configuration.UserDataDirectoryPath, "temp"); ;
         this.temporaryFilesLockPath = Path.Combine(temporaryDirectoryPath, "LOCK");
         this.temporaryFilesLock = new FileLock(temporaryFilesLockPath);
         Directory.CreateDirectory(temporaryDirectoryPath);
      }

      private IDisposable TakeLock() { return temporaryFilesLock.Take(); }
      
      public string AllocateTemporaryDirectory(DateTime expires) {
         using (TakeLock()) {
            var directoryPath = Path.Combine(temporaryDirectoryPath, Guid.NewGuid().ToByteArray().ToHex());
            Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(Path.Combine(directoryPath, EXPIRATION_FILE_NAME), BitConverter.GetBytes(expires.GetUnixTimeMilliseconds()));
            return directoryPath;
         }
      }

      public string AllocateTemporaryFile(string temporaryDirectory, string fileName) 
      {
         using (TakeLock()) {
            var path = Path.Combine(temporaryDirectory, fileName);
            var parentDirectory = path.Substring(0, path.LastIndexOfAny(new[]{ '/', '\\'}));
            Directory.CreateDirectory(parentDirectory);
            File.WriteAllBytes(path, new byte[0]);
            return path;
         }
      }
   }
}