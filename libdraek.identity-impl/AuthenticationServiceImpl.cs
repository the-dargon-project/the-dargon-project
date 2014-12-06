using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Draek.Identities.Dtos;
using Dargon.Draek.Identities.Hydar;
using Dargon.Wyvern.Accounts;

namespace Dargon.Draek.Identities {
   public class AuthenticationServiceImpl : AuthenticationService {
      private readonly AccountService accountService;
      private readonly AuthenticationTokenFactory authenticationTokenFactory;
      private readonly IdentityByTokenCache identityByTokenCache;
      private readonly IAuthenticationServiceConfiguration configuration;

      public AuthenticationServiceImpl(AccountService accountService, AuthenticationTokenFactory authenticationTokenFactory, IdentityByTokenCache identityByTokenCache, IAuthenticationServiceConfiguration configuration) {
         this.accountService = accountService;
         this.authenticationTokenFactory = authenticationTokenFactory;
         this.identityByTokenCache = identityByTokenCache;
         this.configuration = configuration;
      }

      public AuthenticationTokenV1 TryCreateSessionToken(AuthenticationCredentialsV1 credentials) {
         var tokenPrefix = "session";
         var tokenLifetimeMilliseconds = configuration.GetSessionTokenExpirationMilliseconds();
         return CreateAuthenticationTokenOrNull(tokenPrefix, credentials, tokenLifetimeMilliseconds);
      }

      public AuthenticationTokenV1 TryCreateApplicationToken(AuthenticationCredentialsV1 credentials, ApplicationIdentifierV1 applicationIdentifier) {
         var tokenPrefix = "app_" + applicationIdentifier.Name;
         var tokenLifetimeMilliseconds = configuration.GetApplicationTokenExpirationMilliseconds();
         return CreateAuthenticationTokenOrNull(tokenPrefix, credentials, tokenLifetimeMilliseconds);
      }

      public IdentityV1 GetIdentityByTokenV1(AuthenticationTokenV1 authenticationToken) {
         var identity = identityByTokenCache.Get(authenticationToken.Value);
         if (identity == null) {
            return null;
         } else {
            return new IdentityV1(identity.AccountId, identity.AccountName, identity.Email);
         }
      }

      public bool RevokeAuthenticationTokenV1(AuthenticationTokenV1 authenticationToken) {
         return identityByTokenCache.Remove(authenticationToken.Value);
      }

      private AuthenticationTokenV1 CreateAuthenticationTokenOrNull(string tokenPrefix, AuthenticationCredentialsV1 credentials, long tokenLifetimeMillis) {
         var validationResult = accountService.ValidateAccountCredentials(credentials.Email, credentials.HashedPassword);
         if (!validationResult.Success) {
            return AuthenticationTokenV1.kInvalidToken;
         }

         var token = authenticationTokenFactory.CreateToken(tokenPrefix);
         var expirationTime = DateTime.UtcNow.AddMilliseconds(tokenLifetimeMillis);
         var identity = new Identity(validationResult.AccountId, validationResult.AccountName, credentials.Email, expirationTime);
         if (!identityByTokenCache.Put(token, identity)) {
            return AuthenticationTokenV1.kInvalidToken;
         } else {
            return new AuthenticationTokenV1(token);
         }
      }
   }
}
