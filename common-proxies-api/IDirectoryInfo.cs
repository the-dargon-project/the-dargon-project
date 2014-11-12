using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon
{
   public interface IDirectoryInfo
   {
      FileAttributes Attributes { get; set; }
      string FullName { get; }
      string Name { get; }
      bool Exists { get; }
      DateTime CreationTime { get; set; }
      DateTime CreationTimeUtc { get; set; }
      DateTime LastWriteTime { get; set; }
      DateTime LastWriteTimeUtc { get; set; }
   }
}
