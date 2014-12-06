using System;
using System.Text;
using Dargon.Hydar;
using Dargon.Wyvern.Specialized;
using ItzWarty;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class AccountCache : IAccountCache {
      private readonly ICache<string, long> emailToAccountIdCache;
      private readonly ICache<long, AccountInformation> accountInfoByIdCache;
      private readonly IDistributedCounter accountIdCounter;
      private readonly IPasswordUtilities passwordUtilities;

      public AccountCache(ICache<string, long> emailToAccountIdCache, ICache<long, AccountInformation> accountInfoByIdCache, IDistributedCounter accountIdCounter, IPasswordUtilities passwordUtilities) {
         this.emailToAccountIdCache = emailToAccountIdCache;
         this.accountInfoByIdCache = accountInfoByIdCache;
         this.accountIdCounter = accountIdCounter;
         this.passwordUtilities = passwordUtilities;
      }

      public AccountValidationResult ValidateAccountCredentials(string email, string hashedPassword) {
         var emailKey = BuildEmailKey(email);
         long accountId;
         if (!emailToAccountIdCache.TryGetValue(emailKey, out accountId)) {
            return new AccountValidationResult(false);
         }

         AccountInformation accountInformation;
         if (!accountInfoByIdCache.TryGetValue(accountId, out accountInformation)) {
            return new AccountValidationResult(false);
         }

         if (!passwordUtilities.ValidatePassword(hashedPassword, accountInformation.SaltedPassword)) {
            return new AccountValidationResult(false);
         }

         return new AccountValidationResult(true, accountId, accountInformation.Name);
      }

      public AccountInformation GetAccountInformationOrNull(long accountId) {
         return accountInfoByIdCache.GetValueOrDefault(accountId);
      }

      public bool TrySetAccountPassword(long accountId, string hashedPassword) {
         var saltedPassword = passwordUtilities.CreateSaltedHash(hashedPassword);
         return accountInfoByIdCache.Invoke(accountId, new TrySetAccountPasswordProcessor(saltedPassword));
      }

      public bool TryInitializeAccountName(long accountId, string newName) {
         return accountInfoByIdCache.Invoke(accountId, new TryInitializeAccountNameProcessor(newName));
      }
       
      public AccountCreationResult TryCreateAccount(AccountCreationParameters parameters) {
         var rawEmail = parameters.Email;
         var emailKey = BuildEmailKey(rawEmail);
         var saltedPassword = passwordUtilities.CreateSaltedHash(parameters.HashedPassword);

         // atomically reserve email address and set accountid to -1
         if (!emailToAccountIdCache.Invoke(emailKey, new TryReserveEmailProcessor())) {
            return new AccountCreationResult(SpecialAccountIds.kInvalidAccountId,  AccountCreationErrorCodeFlags.EmailExists);
         } else {
            // atomically take unique accountId
            var accountId = accountIdCounter.TakeNextValue();

            // atomically assign accountId to emailToAccountId entry.
            if(!emailToAccountIdCache.Invoke(emailKey, new TryAssignAccountIdProcessor(accountId))) {
               return new AccountCreationResult(SpecialAccountIds.kInvalidAccountId, AccountCreationErrorCodeFlags.ServerError);
            } else {
               // atomically create account information for given accountId.
               if (!accountInfoByIdCache.Invoke(accountId, new AccountCreationProcessor(rawEmail, saltedPassword))) {
                  return new AccountCreationResult(SpecialAccountIds.kInvalidAccountId, AccountCreationErrorCodeFlags.ServerError);
               }
               return new AccountCreationResult(accountId, AccountCreationErrorCodeFlags.Success);
            }
         }
      }

#if DEBUG
      public string DumpCacheContents() {
         var kvps = accountInfoByIdCache.GetAll();
         var sb = new StringBuilder();
         foreach (var kvp in kvps) {
            sb.AppendLine("= Account " + kvp.Key + " =");
            sb.AppendLine("Name:" + kvp.Value.Name);
            sb.AppendLine("Email:" + kvp.Value.Email);
#if DEBUG
            sb.AppendLine("SaltedPassword:" + kvp.Value.SaltedPassword);
#endif
            sb.AppendLine();
         }
         return sb.ToString();
      }
#endif

      internal static string BuildEmailKey(string email) {
         return email.ToLower(); // technically violates RFC 5321 but works for all popular email services.
      }
   }
}
