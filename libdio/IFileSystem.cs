using System.Collections.Generic;

namespace Dargon.FileSystem
{
   public interface IFileSystem
   {
      IFileSystemHandle AllocateRootHandle();
      IoResult AllocateChildrenHandles(IFileSystemHandle handle, out IFileSystemHandle[] childHandles);
      IoResult ReadAllBytes(IFileSystemHandle handle, out byte[] bytes);
      void FreeHandle(IFileSystemHandle handle);
      void FreeHandles(IEnumerable<IFileSystemHandle> handles);
      IoResult GetPath(IFileSystemHandle handle, out string path);

      void Suspend();
      void Resume();
   }
}
