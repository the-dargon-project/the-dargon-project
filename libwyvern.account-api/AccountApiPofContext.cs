using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts {
   public class AccountApiPofContext : PofContext {
      private const int kBasePofId = 1000000;
      public AccountApiPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(AccountCreationParameters));
         RegisterPortableObjectType(kBasePofId + 1, typeof(AccountCreationResult));
         RegisterPortableObjectType(kBasePofId + 2, typeof(AccountValidationResult));
      }
   }
}
