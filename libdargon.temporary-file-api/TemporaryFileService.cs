using System;
using System.Runtime.InteropServices;

namespace Dargon {
   [Guid("380AECFF-6459-4D55-81B9-1A1CDEEFA39A")]
   public interface TemporaryFileService {
      string AllocateTemporaryDirectory(DateTime expires);
      string AllocateTemporaryFile(string temporaryDirectory, string name);
   }
}
