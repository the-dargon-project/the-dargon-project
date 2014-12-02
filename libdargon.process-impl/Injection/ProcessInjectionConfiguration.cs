using Dargon.PortableObjects;

namespace Dargon.Processes.Injection {
   public class ProcessInjectionConfiguration : IProcessInjectionConfiguration, IPortableObject {
      private int injectionAttempts;
      private int injectionAttemptsDelay;

      public ProcessInjectionConfiguration() { }

      public ProcessInjectionConfiguration(int injectionAttempts, int injectionAttemptsDelay) {
         this.injectionAttempts = injectionAttempts;
         this.injectionAttemptsDelay = injectionAttemptsDelay;
      }

      public int InjectionAttempts { get { return injectionAttempts; } }
      public int InjectionAttemptDelay { get { return injectionAttemptsDelay; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, injectionAttempts);
         writer.WriteS32(1, injectionAttemptsDelay);
      }

      public void Deserialize(IPofReader reader) {
         injectionAttempts = reader.ReadS32(0);
         injectionAttemptsDelay = reader.ReadS32(1);
      }
   }
}
