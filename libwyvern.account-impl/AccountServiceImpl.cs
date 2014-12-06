using Dargon.Wyvern.Accounts.Hydar;

namespace Dargon.Wyvern.Accounts {
   public class AccountServiceImpl : AccountService {
      private readonly IAccountCache accountCache;

      public AccountServiceImpl(IAccountCache accountCache) {
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

      public AccountValidationResult ValidateAccountCredentials(string email, string hashedPassword) {
         return accountCache.ValidateAccountCredentials(email, hashedPassword);
      }
   }
}
