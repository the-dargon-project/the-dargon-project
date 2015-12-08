using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Trinkets.Hosted.Hooks;
using ItzWarty.Collections;
using SharpDX;
using SharpDX.Direct3D9;

namespace Dargon.Trinkets.Hosted {
   public class LeagueTextureSwapper {
      private readonly ConcurrentDictionary<IntPtr, Texture> textureSwapsByOriginalTexturePointer = new ConcurrentDictionary<IntPtr, Texture>();
      private readonly Direct3D9HookEventBus direct3D9HookEventBus;
      private readonly LeagueTextureWatcher leagueTextureWatcher;
      private Device device;

      public LeagueTextureSwapper(Direct3D9HookEventBus direct3D9HookEventBus, LeagueTextureWatcher leagueTextureWatcher) {
         this.direct3D9HookEventBus = direct3D9HookEventBus;
         this.leagueTextureWatcher = leagueTextureWatcher;
      }

      public void Initialize() {
         direct3D9HookEventBus.CreateDevicePost += HandleCreateDevice;
         direct3D9HookEventBus.SetTexturePost += HandleSetTexture;
      }

      private void HandleCreateDevice(Direct3D9HookEventBus sender, CreateDevicePostEventArgs e) {
         device = CppObject.FromPointer<Device>(e.ReturnValue);
      }

      private void HandleSetTexture(Direct3D9HookEventBus sender, SetTexturePostEventArgs e) {
         var originalTexture = e.Arguments.PTexture;
         Texture replacementTexture;
         if (textureSwapsByOriginalTexturePointer.TryGetValue(originalTexture, out replacementTexture)) {
            Console.WriteLine("Supposedly swapping texture with override");
            device.SetTexture((int)e.Arguments.Stage, replacementTexture);
         }
      }

      public void SwapTexture(string entryPath, string replacementFilePath) {
         var texture = leagueTextureWatcher.GetLoadedTextureOfEntryPath(entryPath);
         var replacementTexture = Texture.FromFile(texture.Device, replacementFilePath);
         textureSwapsByOriginalTexturePointer.AddOrUpdate(
            texture.NativePointer,
            add => replacementTexture,
            (update, existing) => {
               existing.Dispose();
               return replacementTexture;
            });
      }

      //      public unsafe void FillTextureWithColor(string entryPath, int r, int g, int b) {
      //         Texture textureToReplace = leagueTextureWatcher.GetLoadedTextureOfEntryPath(entryPath);
      //         var device = textureToReplace.Device;
      //         for (var level = 0; level < textureToReplace.LevelCount; level++) {
      //            var levelDescription = textureToReplace.GetLevelDescription(level);
      //            var data = textureToReplace.LockRectangle(level, LockFlags.Discard);
      //            try {
      //               Console.WriteLine($"Level {level}: {levelDescription.Usage} {levelDescription.Format} {levelDescription.Width} {levelDescription.Height}");
      //               for (var y = 0; y < levelDescription.Height; y++) {
      //                  var pCurrentPixel = (byte*)data.DataPointer.ToPointer() + y * data.Pitch;
      //                  for (var x = 0; x < levelDescription.Width; x++) {
      ////                     *(uint*)pCurrentPixel = 0xFFFFFFFF;
      //                     pCurrentPixel += 4;
      //                  }
      //               }
      //            } finally {
      //               textureToReplace.UnlockRectangle(level);
      //            }
      //         }
      //      }

      //      public void SwapTextureWithFile(string entryPath, string replacementTexturePath) {
      //         Texture textureToReplace = leagueTextureWatcher.GetLoadedTextureOfEntryPath(entryPath);
      //         var device = textureToReplace.Device;
      //         using (var replacementTexture = Texture.FromFile(device, replacementTexturePath, Usage.None, Pool.SystemMemory)) {
      //            Console.WriteLine("Dest texture pool: " + textureToReplace.GetLevelDescription(0).Pool);
      //            var dataRectangle = textureToReplace.LockRectangle(0, LockFlags.Discard);
      //
      //            //            textureToReplace.AddDirtyRectangle();
      //            //            device.UpdateTexture(replacementTexture, textureToReplace);
      //         }
      //      }
   }
}
