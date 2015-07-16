using ItzWarty;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ItzWarty.IO;
using NLog;

namespace Dargon {
   public class TemporaryFileServiceImpl : TemporaryFileService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const string EXPIRATION_FILE_NAME = ".expires";
      private readonly IClientConfiguration configuration;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly string temporaryDirectoryRoot;
      private readonly string temporaryFilesLockPath;
      private readonly FileLock temporaryFilesLock;

      public TemporaryFileServiceImpl(IClientConfiguration configuration, IFileSystemProxy fileSystemProxy) {
         this.configuration = configuration;
         this.fileSystemProxy = fileSystemProxy;
         this.temporaryDirectoryRoot = Path.Combine(configuration.UserDataDirectoryPath, "temp"); ;
         this.temporaryFilesLockPath = Path.Combine(temporaryDirectoryRoot, "LOCK");
         this.temporaryFilesLock = new FileLock(temporaryFilesLockPath);
      }

      public void Initialize() {
         fileSystemProxy.PrepareDirectory(temporaryDirectoryRoot);
         var now = DateTime.Now.GetUnixTimeMilliseconds();
         var temporaryDirectories = fileSystemProxy.EnumerateDirectories(temporaryDirectoryRoot);
         foreach (var temporaryDirectory in temporaryDirectories) {
            var temporaryDirectoryInfo = fileSystemProxy.GetDirectoryInfo(temporaryDirectory);
            var expiresFile = temporaryDirectoryInfo.EnumerateFiles(EXPIRATION_FILE_NAME).FirstOrDefault();
            if (expiresFile == null) {
               if (temporaryDirectoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Count() != 0) {
                  logger.Warn($"No {EXPIRATION_FILE_NAME} in temp dir '{temporaryDirectoryInfo.Name}' and not empty - can't collect its garbage!");
               } else {
                  logger.Warn($"Found empty temp dir '{temporaryDirectoryInfo.Name}' - deleting!");
                  try {
                     fileSystemProxy.DeleteDirectory(temporaryDirectoryInfo.FullName, true);
                  } catch (Exception e) {
                     logger.Info($"Error while clearing {temporaryDirectoryInfo.Name}:", e);
                  }
               }
            } else {
               var openedStreams = new List<IFileStream>();
               try {
                  using (var expiresFileStream = expiresFile.Open(FileMode.Open, FileAccess.Read, FileShare.Delete)) {
                     var expirationTime = expiresFileStream.Reader.ReadInt64();
                     if (expirationTime < now) {
                        logger.Info($"{temporaryDirectoryInfo.Name} expires at {expirationTime}, now is {now} so doing nothing.");
                     } else {
                        logger.Info($"{temporaryDirectoryInfo.Name} expired at {expirationTime}, now is {now} attempting to remove.");
                        var files = (from file in temporaryDirectoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                           where !file.Name.Equals(EXPIRATION_FILE_NAME, StringComparison.OrdinalIgnoreCase)
                           select file).ToArray();
                        foreach (var file in files) {
                           openedStreams.Add(file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.Delete));
                        }
                        foreach (var stream in openedStreams) {
                           var path = stream.Path;
                           fileSystemProxy.DeleteFile(path);
                           stream.Close();
                        }
                        fileSystemProxy.DeleteFile(expiresFileStream.Path);
                        expiresFileStream.Close();
                        fileSystemProxy.DeleteDirectory(temporaryDirectory, true);
                        logger.Info($"Removed {temporaryDirectoryInfo.Name}!");
                     }
                  }
               } catch (Exception e) {
                  logger.Info($"Error while clearing {temporaryDirectoryInfo.Name}:", e);
               } finally {
                  foreach (var stream in openedStreams) {
                     stream.Dispose();
                  }
               }
            }
         }
      }

      private IDisposable TakeLock() { return temporaryFilesLock.Take(); }
      
      public string AllocateTemporaryDirectory(TimeSpan expiresIn) {
         using (TakeLock()) {
            var directoryPath = Path.Combine(temporaryDirectoryRoot, Guid.NewGuid().ToByteArray().ToHex());
            Directory.CreateDirectory(directoryPath);
            File.WriteAllBytes(Path.Combine(directoryPath, EXPIRATION_FILE_NAME), BitConverter.GetBytes((DateTime.Now + expiresIn).GetUnixTimeMilliseconds()));
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