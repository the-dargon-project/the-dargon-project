namespace Dargon.FileSystem
{
   public interface IFileSystem
   {
      IFileSystemHandle AllocateRootHandle();
      IoResult AllocateChildrenHandles(IFileSystemHandle handles, out IFileSystemHandle[] childHandles);
      IoResult ReadAllBytes(IFileSystemHandle handle, out byte[] bytes);
      void ReturnHandle(IFileSystemHandle handle);

      void Suspend();
      void Resume();
   }
}
