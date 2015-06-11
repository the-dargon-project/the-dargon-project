using Dargon.PortableObjects;

namespace Dargon.Processes.Injection {
   public class ProcessInjectionConfigurationImpl : ProcessInjectionConfiguration {
      public ProcessInjectionConfigurationImpl() { }

      public ProcessInjectionConfigurationImpl(int injectionAttempts, int injectionAttemptsDelay) {
         this.InjectionAttempts = injectionAttempts;
         this.InjectionAttemptDelay = injectionAttemptsDelay;
      }

      public int InjectionAttempts { get; private set; }
      public int InjectionAttemptDelay { get; private set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, InjectionAttempts);
         writer.WriteS32(1, InjectionAttemptDelay);
      }

      public void Deserialize(IPofReader reader) {
         InjectionAttempts = reader.ReadS32(0);
         InjectionAttemptDelay = reader.ReadS32(1);
      }
   }
}