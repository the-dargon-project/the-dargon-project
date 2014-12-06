using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Draek.Identities.Hydar;
using Dargon.Management;

namespace Dargon.Draek.Identities.Management {
   public class IdentityCacheMob {
      private readonly IdentityByTokenCache identityByTokenCache;

      public IdentityCacheMob(IdentityByTokenCache identityByTokenCache) {
         this.identityByTokenCache = identityByTokenCache;
      }

      [ManagedOperation]
      public string DumpCacheContents() {
         return identityByTokenCache.DumpCacheContents();
      }

      [ManagedOperation]
      public string Get(string token) {
         var identity = identityByTokenCache.Get(token);
         if (identity == null) {
            return null;
         }
         var sb = new StringBuilder();
         sb.AppendLine("Token: " + token);
         sb.AppendLine("AccountId: " + identity.AccountId);
         sb.AppendLine("AccountName: " + identity.AccountName);
         return sb.ToString();
      }

      [ManagedOperation]
      public bool Put(string token, long accountId, string accountName, string email, long tokenLifetimeMillis) {
         var expirationTime = DateTime.UtcNow.AddMilliseconds(tokenLifetimeMillis);
         return identityByTokenCache.Put(token, new Identity(accountId, accountName, email, expirationTime));
      }

      [ManagedOperation]
      public bool Remove(string token) {
         return identityByTokenCache.Remove(token);
      }
   }
}
