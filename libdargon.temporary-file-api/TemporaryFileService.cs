using System;
using System.IO;

namespace Dargon
{
    public interface TemporaryFileService
    {
       IDisposable TakeLock();
       string AllocateTemporaryDirectory(DateTime expires);
       FileStream AllocateTemporaryFile(string temporaryDirectory, string name);
    }
}
