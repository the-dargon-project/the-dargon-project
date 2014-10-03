namespace Dargon.Processes.Injection
{
   public class ProcessInjectionConfiguration : IProcessInjectionConfiguration
   {
      private readonly int injectionAttempts;
      private readonly int injectionAttemptsDelay;

      public ProcessInjectionConfiguration(int injectionAttempts, int injectionAttemptsDelay)
      {
         this.injectionAttempts = injectionAttempts;
         this.injectionAttemptsDelay = injectionAttemptsDelay;
      }

      public int InjectionAttempts { get { return injectionAttempts; } }
      public int InjectionAttemptDelay { get { return injectionAttemptsDelay; } }
   }
}