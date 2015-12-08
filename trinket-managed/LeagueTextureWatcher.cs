using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dargon.RADS;
using Dargon.RADS.Archives;
using Dargon.Trinkets.Hosted.Hooks;
using ItzWarty.Collections;
using SharpDX;
using SharpDX.Direct3D9;

namespace Dargon.Trinkets.Hosted {
   public class LeagueTextureWatcher {
      private readonly ConcurrentDictionary<string, Texture> texturesByEntryPath = new ConcurrentDictionary<string, Texture>();
      private readonly Direct3D9HookEventBus direct3D9HookEventBus;
      private readonly LeagueRadsWatcher leagueRadsWatcher;
      private RafEntry lastReadEntry;

      public LeagueTextureWatcher(Direct3D9HookEventBus direct3D9HookEventBus, LeagueRadsWatcher leagueRadsWatcher) {
         this.direct3D9HookEventBus = direct3D9HookEventBus;
         this.leagueRadsWatcher = leagueRadsWatcher;
      }

      public IReadOnlyDictionary<string, Texture> TexturesByEntryPath => texturesByEntryPath;

      public void Initialize() {
         leagueRadsWatcher.RadsEntryRead += HandleRadsEntryRead;
         direct3D9HookEventBus.CreateTexturePost += CreateTextureHandler;
      }

      private void HandleRadsEntryRead(LeagueRadsWatcher sender, RafEntry entry) {
         lastReadEntry = entry;
      }

      private void CreateTextureHandler(Direct3D9HookEventBus sender, CreateTexturePostEventArgs e) {
         var entry = Interlocked.Exchange(ref lastReadEntry, null);
         var ppTexture = e.Arguments.PPTexture;
         if (entry != null && ppTexture != IntPtr.Zero) {
            var pTexture = (IntPtr)Marshal.PtrToStructure(ppTexture, typeof(IntPtr));
            Console.WriteLine($"Guessing that texture at ppTexture {ppTexture} pTexture {pTexture} is {entry.Path}.");
            var texture = CppObject.FromPointer<Texture>(pTexture);
            texturesByEntryPath.AddOrUpdate(
               entry.Path,
               add => texture,
               (update, existing) => texture);
         }
      }

      public Texture GetLoadedTextureOfEntryPath(string entryPath) {
         return texturesByEntryPath[entryPath];
      }

      public bool TryGetLoadedTextureOfEntryPath(string entryPath, out Texture texture) {
         return texturesByEntryPath.TryGetValue(entryPath, out texture);
      }
   }
}
