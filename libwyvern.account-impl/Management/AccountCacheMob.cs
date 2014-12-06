using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Management;
using Dargon.Wyvern.Accounts.Hydar;

namespace Dargon.Wyvern.Accounts.Management {
   public class AccountCacheMob {
      private readonly AccountCache accountCache;

      public AccountCacheMob(AccountCache accountCache) {
         this.accountCache = accountCache;
      }

      [ManagedOperation]
      public string ValidateAccountCredentials(string email, string hashedPassword) {
         var validationResult = accountCache.ValidateAccountCredentials(email, hashedPassword);
         var sb = new StringBuilder();
         sb.AppendLine("Success: " + validationResult.Success);
         sb.AppendLine("AccountId: " + validationResult.AccountId);
         sb.AppendLine("AccountName: " + validationResult.AccountName);
         return sb.ToString();
      }

      [ManagedOperation]
      public string GetAccountInformationOrNull(long accountId) {
         var result = accountCache.GetAccountInformationOrNull(accountId);
         if (result == null) {
            return null;
         } else {
            var sb = new StringBuilder();
            sb.AppendLine("Name: " + result.Name);
            sb.AppendLine("Email: " + result.Email);
#if DEBUG
            sb.AppendLine("SaltedPassword: " + result.SaltedPassword);
#endif
            return sb.ToString();
         }
      }

      [ManagedOperation]
      public bool TrySetAccountPassword(long accountId, string hashedPassword) {
         return accountCache.TrySetAccountPassword(accountId, hashedPassword);
      }

      [ManagedOperation]
      public bool TryInitializeAccountName(long accountId, string newName) {
         return accountCache.TryInitializeAccountName(accountId, newName);
      }

      [ManagedOperation]
      public string TryCreateAccount(string email, string hashedPassword) {
         var accountCreationParameters = new AccountCreationParameters(email, hashedPassword);
         var result = accountCache.TryCreateAccount(accountCreationParameters);
         var sb = new StringBuilder();
         sb.AppendLine("AccountId: " + result.AccountId);
         sb.AppendLine("ErrorCodeFlags: " + result.ErrorCodeFlags);
         return sb.ToString();
      }

#if DEBUG
      [ManagedOperation]
      public string DumpCacheContents() {
         return accountCache.DumpCacheContents();
      }
#endif
   }
}
