using System.Text;
using Dargon.Management;

namespace Dargon.Draek.Identities.Management {
   public class AuthenticationServiceConfigurationMob {
      private readonly IAuthenticationServiceConfiguration configuration;

      public AuthenticationServiceConfigurationMob(IAuthenticationServiceConfiguration configuration) {
         this.configuration = configuration;
      }

      [ManagedOperation]
      public string GetConfiguration() {
         var sb = new StringBuilder();
         sb.AppendLine("SessionTokenExpirationMilliseconds: " + GetSessionTokenExpirationMilliseconds());
         sb.AppendLine("ApplicationTokenExpirationMilliseconds: " + GetApplicationTokenExpirationMilliseconds());
         return sb.ToString();
      }

      [ManagedOperation]
      public string ResetConfiguration() {
         var sb = new StringBuilder();
         sb.AppendLine(
            "SessionTokenExpirationMilliseconds: " + GetSessionTokenExpirationMilliseconds() + 
            " => " + AuthenticationServiceConfiguration.kDefaultSessionTokenExpirationMilliseconds
         );
         SetSessionTokenExpirationMilliseconds(AuthenticationServiceConfiguration.kDefaultSessionTokenExpirationMilliseconds);

         sb.AppendLine(
            "ApplicationTokenExpirationMilliseconds: " + GetApplicationTokenExpirationMilliseconds() + 
            " => " + AuthenticationServiceConfiguration.kDefaultApplicationTokenExpirationMilliseconds
         );
         SetApplicationTokenExpirationMilliseconds(AuthenticationServiceConfiguration.kDefaultApplicationTokenExpirationMilliseconds);
         return sb.ToString();
      }

      [ManagedOperation]
      public long GetSessionTokenExpirationMilliseconds() {
         return configuration.GetSessionTokenExpirationMilliseconds();
      }

      [ManagedOperation]
      public void SetSessionTokenExpirationMilliseconds(long value) {
         configuration.SetSessionTokenExpirationMilliseconds(value);
      }

      [ManagedOperation]
      public long GetApplicationTokenExpirationMilliseconds() {
         return configuration.GetApplicationTokenExpirationMilliseconds();
      }

      [ManagedOperation]
      public void SetApplicationTokenExpirationMilliseconds(long value) {
         configuration.SetApplicationTokenExpirationMilliseconds(value);
      }
   }
}
