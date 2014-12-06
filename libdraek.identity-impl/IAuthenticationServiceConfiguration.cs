using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Draek.Identities {
   public interface IAuthenticationServiceConfiguration {
      long GetSessionTokenExpirationMilliseconds();
      void SetSessionTokenExpirationMilliseconds(long value);
      long GetApplicationTokenExpirationMilliseconds();
      void SetApplicationTokenExpirationMilliseconds(long value);
   }
}
