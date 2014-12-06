using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Wyvern.Accounts {
   [Flags]
   public enum AccountCreationErrorCodeFlags {
      Success                    = 0x0000000,
      EmailExists                = 0x0000001,

      ServerError                = 0x0001000,
   }
}
