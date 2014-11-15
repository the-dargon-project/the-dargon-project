using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon
{
   public interface IFileSystemProxy
   {
      IEnumerable<string> EnumerateDirectories(string path);
      IEnumerable<string> EnumerateDirectories(string path, string searchPattern);
      IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

      IDirectoryInfo GetDirectoryInfo(string path);
      
      string ReadAllText(string path);
      void WriteAllText(string path, string contents);
      void CopyFile(string sourceFilePath, string destinationFilePath);

      void PrepareDirectory(string path);
      void PrepareParentDirectory(string path);

      void DeleteDirectory(string path, bool recursive = false);
   }
}
