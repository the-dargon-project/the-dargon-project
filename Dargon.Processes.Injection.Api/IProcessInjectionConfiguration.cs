namespace Dargon.Processes.Injection {
   public interface ProcessInjectionConfiguration {
      int InjectionAttempts { get; }
      int InjectionAttemptDelay { get; }
   }
}