using Dargon.Management;
using Fody.Constructors;
using System;
using System.Text;

namespace Dargon.Trinkets.Hosted.Management.Game {
   [RequiredFieldsConstructor]
   public class LeagueTextureSwapperMob {
      private readonly LeagueTextureWatcher leagueTextureWatcher = null;
      private readonly LeagueTextureSwapper leagueTextureSwapper = null;

      [ManagedProperty]
      public int WatchedTextureCount => leagueTextureWatcher.TexturesByEntryPath.Count;

      [ManagedOperation]
      public string SearchTexturesByPath(string entryPathPart) {
         var sb = new StringBuilder();
         foreach (var kvp in leagueTextureWatcher.TexturesByEntryPath) {
            if (kvp.Key.IndexOf(entryPathPart, StringComparison.OrdinalIgnoreCase) != -1) {
               sb.AppendLine(kvp.Key);
            }
         }
         return sb.ToString();
      }

//      [ManagedOperation]
//      public string FillTextureWithColor(string entryPath, int r, int g, int b) {
//         try {
//            leagueTextureSwapper.FillTextureWithColor(entryPath, r, g, b);
//            return "Success!";
//         } catch (Exception e) {
//            Console.WriteLine(e);
//            return e.ToString();
//         }
//      }

      [ManagedOperation]
      public string SwapTextureWithFile(string entryPath, string replacementTexturePath) {
         try {
            leagueTextureSwapper.SwapTexture(entryPath, replacementTexturePath);
            return "Success!";
         } catch (Exception e) {
            Console.WriteLine(e);
            return e.ToString();
         }
      }
   }
}
