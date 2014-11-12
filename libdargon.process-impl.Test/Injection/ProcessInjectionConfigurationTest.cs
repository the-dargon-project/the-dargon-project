using NMockito;
using Xunit;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionConfigurationTest : NMockitoInstance
   {
      private ProcessInjectionConfiguration testObj = new ProcessInjectionConfiguration(INJECTION_ATTEMPTS, INJECTION_ATTEMPTS_DELAY);
      
      private const int INJECTION_ATTEMPTS = 1209831;
      private const int INJECTION_ATTEMPTS_DELAY = 4385402;

      [Fact]
      public void PropertiesReflectConstructorArgumentsTest()
      {
         AssertEquals(INJECTION_ATTEMPTS, testObj.InjectionAttempts);
         AssertEquals(INJECTION_ATTEMPTS_DELAY, testObj.InjectionAttemptDelay);
      }
   }
}
