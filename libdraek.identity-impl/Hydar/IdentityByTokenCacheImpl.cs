using System.Text;
using Dargon.Hydar;
using ItzWarty;

namespace Dargon.Draek.Identities.Hydar {
   public class IdentityByTokenCacheImpl : IdentityByTokenCache {
      private readonly ICache<string, Identity> cache;

      public IdentityByTokenCacheImpl(ICache<string, Identity> cache) {
         this.cache = cache;
      }

      public bool Put(string token, Identity value) {
         cache[token] = value;
         return true;
      }

      public Identity Get(string token) {
         return cache.GetValueOrDefault(token);
      }

      public bool Remove(string token) {
         return cache.Remove(token);
      }

      public string DumpCacheContents() {
         var result = new StringBuilder();
         var kvps = cache.GetAll();
         foreach (var kvp in kvps) {
            result.AppendLine("Token: " + kvp.Key);
            result.AppendLine("AccountId: " + kvp.Value.AccountId);
            result.AppendLine("AccountName: " + kvp.Value.AccountName);
         }
         return result.ToString();
      }
   }
}