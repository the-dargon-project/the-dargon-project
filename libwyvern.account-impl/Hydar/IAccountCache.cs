using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Wyvern.Accounts.Hydar {
   public interface IAccountCache {
      AccountValidationResult ValidateAccountCredentials(string hashedPassword, string email);
      AccountInformation GetAccountInformationOrNull(long accountId);
      bool TrySetAccountPassword(long accountId, string hashedPassword);
      bool TryInitializeAccountName(long accountId, string newName);
      AccountCreationResult TryCreateAccount(AccountCreationParameters parameters);

#if DEBUG
      string DumpCacheContents();
#endif
   }
}
