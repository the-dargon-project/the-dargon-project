using System;
using System.IO;
using Dargon.Daemon;
using ItzWarty;

namespace Dargon
{
   public class TemporaryFileServiceImpl : TemporaryFileService
   {
      private const string EXPIRATION_FILE_NAME = ".expires";
      private readonly IDaemonConfiguration configuration;
      private readonly string temporaryDirectoryPath;
      private readonly string temporaryFilesLockPath;
      private readonly FileLock temporaryFilesLock;

      public TemporaryFileServiceImpl(IDaemonConfiguration configuration) {
         this.configuration = configuration;
         this.temporaryDirectoryPath = Path.Combine(configuration.UserDataDirectoryPath, "temp"); ;
         this.temporaryFilesLockPath = Path.Combine(temporaryDirectoryPath, "LOCK");
         this.temporaryFilesLock = new FileLock(temporaryFilesLockPath);
         Util.PrepareDirectory(temporaryDirectoryPath);
      }

      public IDisposable TakeLock() { return temporaryFilesLock.Take(); }
      
      public string AllocateTemporaryDirectory(DateTime expires) {
         using (TakeLock()) {
            var directoryPath = Path.Combine(temporaryDirectoryPath, Guid.NewGuid().ToByteArray().ToHex());
            Util.PrepareDirectory(directoryPath);
            File.WriteAllBytes(Path.Combine(directoryPath, EXPIRATION_FILE_NAME), BitConverter.GetBytes(expires.GetUnixTimeMilliseconds()));
            return directoryPath;
         }
      }

      public FileStream AllocateTemporaryFile(string temporaryDirectory, string fileName) 
      {
         using (TakeLock()) {
            var path = Path.Combine(temporaryDirectory, fileName);
            Util.PrepareParentDirectory(path);
            return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
         }
      }
   }
}