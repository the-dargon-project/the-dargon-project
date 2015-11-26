using System;
using System.IO;

namespace Dargon.Trinkets.Hosted.Hooks {
   public interface FileSystemHookEventBus {
      event FileSystemHookEventHandler<CreateFilePreEventArgs> CreateFilePre;

      void RaiseCreateFilePre(CreateFilePreEventArgs e);
   }

   public class FileSystemHookEventBusImpl : FileSystemHookEventBus {
      public event FileSystemHookEventHandler<CreateFilePreEventArgs> CreateFilePre;

      public void RaiseCreateFilePre(CreateFilePreEventArgs e) => CreateFilePre?.Invoke(this, e);
   }

   public delegate void FileSystemHookEventHandler<T>(FileSystemHookEventBus sender, T e);

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
}
