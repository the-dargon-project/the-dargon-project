namespace Dargon.Draek.Identities {
   public class AuthenticationServiceConfiguration : IAuthenticationServiceConfiguration {
      public const string kKeySessionTokenExpirationMilliseconds = "dargon.draek.identities.session_auth_token_expiration";
      public const string kKeyApplicationTokenExpirationMilliseconds = "dargon.draek.identities.application_auth_token_expiration";
      public const long kDefaultSessionTokenExpirationMilliseconds = 2L * 60L * 60L * 1000L; // 2 hours
      public const long kDefaultApplicationTokenExpirationMilliseconds = -1; // forever
      private readonly SystemState systemState;

      public AuthenticationServiceConfiguration(SystemState systemState) {
         this.systemState = systemState;
      }

      public long GetSessionTokenExpirationMilliseconds() {
         return GetTokenExpirationMillisecondsInternal(
            kKeySessionTokenExpirationMilliseconds, 
            kDefaultSessionTokenExpirationMilliseconds
            );
      }

      public void SetSessionTokenExpirationMilliseconds(long value) {
         SetTokenExpirationMillisecondInternal(kKeySessionTokenExpirationMilliseconds, value);
      }

      public long GetApplicationTokenExpirationMilliseconds() {
         return GetTokenExpirationMillisecondsInternal(
            kKeyApplicationTokenExpirationMilliseconds,
            kDefaultApplicationTokenExpirationMilliseconds
            );
      }

      public void SetApplicationTokenExpirationMilliseconds(long value) {
         SetTokenExpirationMillisecondInternal(kKeyApplicationTokenExpirationMilliseconds, value);
      }

      private long GetTokenExpirationMillisecondsInternal(string key, long defaultValue) {
         var stringValue = systemState.Get(key, null);
         long value;
         if (string.IsNullOrWhiteSpace(stringValue) || !long.TryParse(stringValue, out value)) {
            return defaultValue;
         } else {
            return value;
         }
      }

      private void SetTokenExpirationMillisecondInternal(string key, long value) {
         systemState.Set(key, value.ToString());
      }
   }
}