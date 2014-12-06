using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Draek.Identities.Dtos;
using Dargon.Management;

namespace Dargon.Draek.Identities.Management {
   public class AuthenticationServiceMob {
      private readonly AuthenticationService authenticationService;

      public AuthenticationServiceMob(AuthenticationService authenticationService) {
         this.authenticationService = authenticationService;
      }

      [ManagedOperation]
      public string TryCreateSessionToken(string email, string hashedPassword) {
         var credentials = new AuthenticationCredentialsV1(email, hashedPassword);
         var result = authenticationService.TryCreateSessionToken(credentials);
         var sb = new StringBuilder();
         sb.AppendLine("Token: " + result.Value);
         sb.AppendLine("IsValid: " + result.IsValid);
         return sb.ToString();
      }

      [ManagedOperation]
      public string TryCreateApplicationToken(string email, string hashedPassword, string applicationName) {
         var applicationIdentifier = new ApplicationIdentifierV1(applicationName);
         var credentials = new AuthenticationCredentialsV1(email, hashedPassword);
         var result = authenticationService.TryCreateApplicationToken(credentials, applicationIdentifier);
         var sb = new StringBuilder();
         sb.AppendLine("Token: " + result.Value);
         sb.AppendLine("IsValid: " + result.IsValid);
         return sb.ToString();
      }

      [ManagedOperation]
      public string GetIdentityByToken(string token) {
         var identity = authenticationService.GetIdentityByTokenV1(new AuthenticationTokenV1(token));
         if (identity == null) {
            return null;
         }
         var sb = new StringBuilder();
         sb.AppendLine("AccountId: " + identity.AccountId);
         sb.AppendLine("AccountName: " + identity.AccountName);
         sb.AppendLine("Email: " + identity.Email);
         return sb.ToString();
      }

      [ManagedOperation]
      public bool RevokeToken(string token) {
         return authenticationService.RevokeAuthenticationTokenV1(new AuthenticationTokenV1(token));
      }
   }
}
