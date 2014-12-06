using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Wyvern.Accounts {
   public interface AccountService {
      bool TrySetAccountPassword(long accountId, string password);
      bool TryInitializeAccountName(long accountId, string newName);
      AccountCreationResult TryCreateAccount(AccountCreationParameters parameters);
      AccountValidationResult ValidateAccountCredentials(string email, string hashedPassword);
   }
}
