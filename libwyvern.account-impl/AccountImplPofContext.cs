using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Wyvern.Accounts.Hydar;

namespace Dargon.Wyvern.Accounts {
   public class AccountImplPofContext : PofContext {
      public const int kBasePofId = 1001000;

      public AccountImplPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(TryAssignAccountIdProcessor));
         RegisterPortableObjectType(kBasePofId + 1, typeof(TryInitializeAccountNameProcessor));
         RegisterPortableObjectType(kBasePofId + 2, typeof(TryReserveEmailProcessor));
         RegisterPortableObjectType(kBasePofId + 3, typeof(TrySetAccountPasswordProcessor));
      }
   }
}
