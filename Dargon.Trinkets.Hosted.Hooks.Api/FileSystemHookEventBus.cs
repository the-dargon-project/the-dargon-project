using System;
using System.IO;

namespace Dargon.Trinkets.Hosted.Hooks {
   public interface FileSystemHookEventBus {
      event FileSystemHookEventHandler<CreateFilePreEventArgs> CreateFilePre;
      event FileSystemHookEventHandler<CreateFilePostEventArgs> CreateFilePost;

      event FileSystemHookEventHandler<ReadFilePreEventArgs> ReadFilePre;
      event FileSystemHookEventHandler<ReadFilePostEventArgs> ReadFilePost;

      event FileSystemHookEventHandler<CloseHandlePreEventArgs> CloseHandlePre;
      event FileSystemHookEventHandler<CloseHandlePostEventArgs> CloseHandlePost;

      void RaiseCreateFilePre(CreateFilePreEventArgs e);
      void RaiseCreateFilePost(CreateFilePostEventArgs e);

      void RaiseReadFilePre(ReadFilePreEventArgs e);
      void RaiseReadFilePost(ReadFilePostEventArgs e);

      void RaiseCloseHandlePre(CloseHandlePreEventArgs e);
      void RaiseCloseHandlePost(CloseHandlePostEventArgs e);
   }

   public class FileSystemHookEventBusImpl : FileSystemHookEventBus {
      public event FileSystemHookEventHandler<CreateFilePreEventArgs> CreateFilePre;
      public event FileSystemHookEventHandler<CreateFilePostEventArgs> CreateFilePost;

      public event FileSystemHookEventHandler<ReadFilePreEventArgs> ReadFilePre;
      public event FileSystemHookEventHandler<ReadFilePostEventArgs> ReadFilePost;

      public event FileSystemHookEventHandler<CloseHandlePreEventArgs> CloseHandlePre;
      public event FileSystemHookEventHandler<CloseHandlePostEventArgs> CloseHandlePost;

      public void RaiseCreateFilePre(CreateFilePreEventArgs e) => CreateFilePre?.Invoke(this, e);
      public void RaiseCreateFilePost(CreateFilePostEventArgs e) => CreateFilePost?.Invoke(this, e);

      public void RaiseReadFilePre(ReadFilePreEventArgs e) => ReadFilePre?.Invoke(this, e);
      public void RaiseReadFilePost(ReadFilePostEventArgs e) => ReadFilePost?.Invoke(this, e);

      public void RaiseCloseHandlePre(CloseHandlePreEventArgs e) => CloseHandlePre?.Invoke(this, e);
      public void RaiseCloseHandlePost(CloseHandlePostEventArgs e) => CloseHandlePost?.Invoke(this, e);
   }

   public delegate void FileSystemHookEventHandler<T>(FileSystemHookEventBus sender, T e);

   public class CloseHandleArguments {
      public CloseHandleArguments(IntPtr handle) {
         Handle = handle;
      }

      public IntPtr Handle { get; private set; }
   }

   public class CloseHandlePreEventArgs : EventArgs {
      public CloseHandlePreEventArgs(CloseHandleArguments arguments) {
         Arguments = arguments;
      }

      public CloseHandleArguments Arguments { get; private set; }
   }

   public unsafe class CloseHandlePostEventArgs : EventArgs {
      public CloseHandlePostEventArgs(CloseHandleArguments arguments, uint returnValue) {
         Arguments = arguments;
         ReturnValue = returnValue;
      }

      public CloseHandleArguments Arguments { get; private set; }
      public uint ReturnValue { get; private set; }
   }

   public class CreateFileArguments {
      public CreateFileArguments(string path, FileAccess access, FileShare share, FileMode mode) {
         Path = path;
         Access = access;
         Share = share;
         Mode = mode;
      }

      public string Path { get; private set; }
      public FileAccess Access { get; private set; }
      public FileShare Share { get; private set; }
      public FileMode Mode { get; private set; }
   }

   public class CreateFilePreEventArgs : EventArgs {
      public CreateFilePreEventArgs(CreateFileArguments arguments) {
         Arguments = arguments;
      }

      public CreateFileArguments Arguments { get; private set; }
   }

   public unsafe class CreateFilePostEventArgs : EventArgs {
      public CreateFilePostEventArgs(CreateFileArguments arguments, IntPtr returnValue) {
         Arguments = arguments;
         ReturnValue = returnValue;
      }

      public CreateFileArguments Arguments { get; private set; }
      public IntPtr ReturnValue { get; private set; }
   }

   public unsafe class ReadFileArguments {
      public ReadFileArguments(IntPtr handle, byte* buffer, uint numberOfBytesToRead, uint* lpNumberOfBytesRead, ulong fileOffset) {
         Handle = handle;
         Buffer = buffer;
         NumberOfBytesToRead = numberOfBytesToRead;
         LpNumberOfBytesRead = lpNumberOfBytesRead;
         FileOffset = fileOffset;
      }

      public IntPtr Handle { get; private set; }
      public byte* Buffer { get; private set; }
      public uint NumberOfBytesToRead { get; private set; }
      public uint* LpNumberOfBytesRead { get; private set; }
      public ulong FileOffset { get; private set; }
   }

   public class ReadFilePreEventArgs : EventArgs {
      public ReadFilePreEventArgs(ReadFileArguments arguments) {
         Arguments = arguments;
      }

      public ReadFileArguments Arguments { get; private set; }
   }

   public class ReadFilePostEventArgs : EventArgs {
      public ReadFilePostEventArgs(ReadFileArguments arguments, uint returnValue) {
         Arguments = arguments;
         ReturnValue = returnValue;
      }

      public ReadFileArguments Arguments { get; private set; }
      public uint ReturnValue { get; private set; }
   }
}
