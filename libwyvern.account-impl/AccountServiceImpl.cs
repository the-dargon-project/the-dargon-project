using Dargon.Wyvern.Accounts.Hydar;

namespace Dargon.Wyvern.Accounts {
   public class AccountServiceImpl : AccountService {
      private readonly AccountCache accountCache;

      public AccountServiceImpl(AccountCache accountCache) {
         this.accountCache = accountCache;
      }

      public bool TrySetAccountPassword(long accountId, string password) {
         return accountCache.TrySetAccountPassword(accountId, password);
      }

      public bool TryInitializeAccountName(long accountId, string newName) {
         return accountCache.TryInitializeAccountName(accountId, newName);
      }

      public AccountCreationResult TryCreateAccount(AccountCreationParameters parameters) {
         return accountCache.TryCreateAccount(parameters);
      }
   }
}
