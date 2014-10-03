namespace Dargon.Processes.Injection
{
   public interface IProcessInjectionConfiguration
   {
      int InjectionAttempts { get; }
      int InjectionAttemptDelay { get; }
   }
}