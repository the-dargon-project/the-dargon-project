using Dargon.Draek.Identities.Dtos;

namespace Dargon.Draek.Identities {
   public class IdentityServiceProxyImpl : IdentityService {
      private readonly AuthenticationService authenticationService;

      public IdentityServiceProxyImpl(AuthenticationService authenticationService) {
         this.authenticationService = authenticationService;
      }

      public AuthenticationTokenV1 TryCreateSessionToken(AuthenticationCredentialsV1 credentials) {
         return authenticationService.TryCreateSessionToken(credentials);
      }

      public AuthenticationTokenV1 TryCreateApplicationToken(AuthenticationCredentialsV1 credentials, ApplicationIdentifierV1 applicationIdentifier) {
         return authenticationService.TryCreateApplicationToken(credentials, applicationIdentifier);
      }

      public IdentityV1 GetIdentityByTokenV1(AuthenticationTokenV1 authenticationToken) {
         return authenticationService.GetIdentityByTokenV1(authenticationToken);
      }

      public bool RevokeAuthenticationTokenV1(AuthenticationTokenV1 authenticationToken) {
         return authenticationService.RevokeAuthenticationTokenV1(authenticationToken);
      }
   }
}
