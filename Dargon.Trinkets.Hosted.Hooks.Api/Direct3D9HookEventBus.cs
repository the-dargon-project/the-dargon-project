using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Trinkets.Hosted.Hooks {
   public interface Direct3D9HookEventBus {
      event Direct3D9HookEventHandler<CreateDevicePostEventArgs> CreateDevicePost;
      event Direct3D9HookEventHandler<CreateTexturePostEventArgs> CreateTexturePost;
      event Direct3D9HookEventHandler<SetTexturePostEventArgs> SetTexturePost;

      void RaiseCreateDevicePost(CreateDevicePostEventArgs e);
      void RaiseCreateTexturePost(CreateTexturePostEventArgs e);
      void RaiseSetTexturePost(SetTexturePostEventArgs e);
   }

   public class Direct3D9HookEventBusImpl : Direct3D9HookEventBus {
      public event Direct3D9HookEventHandler<CreateDevicePostEventArgs> CreateDevicePost;
      public event Direct3D9HookEventHandler<CreateTexturePostEventArgs> CreateTexturePost;
      public event Direct3D9HookEventHandler<SetTexturePostEventArgs> SetTexturePost;

      public void RaiseCreateDevicePost(CreateDevicePostEventArgs e) => CreateDevicePost?.Invoke(this, e);
      public void RaiseCreateTexturePost(CreateTexturePostEventArgs e) => CreateTexturePost?.Invoke(this, e);
      public void RaiseSetTexturePost(SetTexturePostEventArgs e) => SetTexturePost?.Invoke(this, e);
   }

   public delegate void Direct3D9HookEventHandler<T>(Direct3D9HookEventBus sender, T e);

   public class CreateDevicePostEventArgs : EventArgs {
      public CreateDevicePostEventArgs(IntPtr returnValue) {
         ReturnValue = returnValue;
      }

      public IntPtr ReturnValue { get; private set; }
   }

   public class CreateTextureArguments {
      public CreateTextureArguments(uint width, uint height, uint levels, uint usage, int format, int pool, IntPtr ppTexture, IntPtr sharedHandle) {
         Width = width;
         Height = height;
         Levels = levels;
         Usage = usage;
         Format = format;
         Pool = pool;
         PPTexture = ppTexture;
         SharedHandle = sharedHandle;
      }

      public uint Width { get; private set; }
      public uint Height { get; private set; }
      public uint Levels { get; private set; }
      public uint Usage { get; private set; }
      public int Format { get; private set; }
      public int Pool { get; private set; }
      public IntPtr PPTexture { get; private set; }
      public IntPtr SharedHandle { get; private set; }
   }

   public class CreateTexturePostEventArgs : EventArgs {
      public CreateTexturePostEventArgs(CreateTextureArguments arguments, uint returnValue) {
         Arguments = arguments;
         ReturnValue = returnValue;
      }

      public CreateTextureArguments Arguments { get; private set; }
      public uint ReturnValue { get; private set; }
   }

   public class SetTextureArguments {
      public SetTextureArguments(uint stage, IntPtr pTexture) {
         Stage = stage;
         PTexture = pTexture;
      }

      public uint Stage { get; private set; }
      public IntPtr PTexture { get; private set; }
   }

   public class SetTexturePostEventArgs : EventArgs {
      public SetTexturePostEventArgs(SetTextureArguments arguments, uint returnValue) {
         Arguments = arguments;
         ReturnValue = returnValue;
      }

      public SetTextureArguments Arguments { get; private set; }
      public uint ReturnValue { get; private set; }
   }

   /*
   struct CreateTextureArgs {
      std::uint32_t width;
      std::uint32_t height;
      std::uint32_t levels;
      std::uint32_t usage;
      std::int32_t format;
      std::int32_t pool;
      void** ppTexture;
      void* pSharedHandle;
   };
*/
}
