using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Draek.Identities.Dtos;

namespace Dargon.Draek.Identities {
   public interface AuthenticationService {
      AuthenticationTokenV1 TryCreateSessionToken(AuthenticationCredentialsV1 credentials);
      AuthenticationTokenV1 TryCreateApplicationToken(AuthenticationCredentialsV1 credentials, ApplicationIdentifierV1 applicationIdentifier);
      IdentityV1 GetIdentityByTokenV1(AuthenticationTokenV1 authenticationToken);
      bool RevokeAuthenticationTokenV1(AuthenticationTokenV1 authenticationToken);
   }
}
