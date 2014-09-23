namespace Dargon.FileSystem
{
   public interface IFileSystemHandle {
      HandleState State { get; }
   }

   public enum HandleState
   {
      // handle in valid state - resource exists
      Valid,

      // handle in invalidated state - resource no longer exists
      Invalidated,

      // handle in reset state - resource re-lookup required, resource may or may not exist.
      Reset,

      // handle in disposed state
      Disposed
   }
}