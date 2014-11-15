using ImpromptuInterface;
using ItzWarty;
using System.Collections.Generic;
using System.IO;

namespace Dargon
{
   public class FileSystemProxy : IFileSystemProxy
   {
      public IEnumerable<string> EnumerateDirectories(string path) { return Directory.EnumerateDirectories(path); }
      public IEnumerable<string> EnumerateDirectories(string path, string searchPattern) { return Directory.EnumerateDirectories(path, searchPattern); }
      public IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) { return Directory.EnumerateDirectories(path, searchPattern, searchOption); }

      public IDirectoryInfo GetDirectoryInfo(string path) { return WrapDirectoryInfo(new DirectoryInfo(path)); }
      public string ReadAllText(string path) { return File.ReadAllText(path); }
      public void WriteAllText(string path, string contents) {  File.WriteAllText(path, contents);}
      public void CopyFile(string sourceFilePath, string destinationFilePath) { File.Copy(sourceFilePath, destinationFilePath); }

      public void PrepareDirectory(string path) { Util.PrepareDirectory(path); }
      public void PrepareParentDirectory(string path) { Util.PrepareParentDirectory(path); }

      public void DeleteDirectory(string path, bool recursive = false) {
         if (!recursive) {
            Directory.Delete(path, false);
         } else {
            foreach (var dir in Directory.EnumerateDirectories(path)) {
               DeleteDirectory(dir, true);
            }
            foreach (var file in Directory.EnumerateFiles(path)) {
               File.SetAttributes(file, FileAttributes.Normal);
               File.Delete(file);
            }
            Directory.Delete(path, false);
         }
      }

      private IDirectoryInfo WrapDirectoryInfo(DirectoryInfo directoryInfo) { return directoryInfo.ActLike<IDirectoryInfo>(); }
   }
}