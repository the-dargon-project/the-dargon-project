using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMockito;

namespace Dargon.Processes.Injection
{
   [TestClass]
   public class ProcessInjectionConfigurationTest : NMockitoInstance
   {
      private ProcessInjectionConfiguration testObj;
      
      private const int INJECTION_ATTEMPTS = 1209831;
      private const int INJECTION_ATTEMPTS_DELAY = 4385402;

      [TestInitialize]
      public void Setup()
      {
         InitializeMocks();

         testObj = new ProcessInjectionConfiguration(INJECTION_ATTEMPTS, INJECTION_ATTEMPTS_DELAY);
      }

      [TestMethod]
      public void PropertiesReflectConstructorArgumentsTest()
      {
         AssertEquals(INJECTION_ATTEMPTS, testObj.InjectionAttempts);
         AssertEquals(INJECTION_ATTEMPTS_DELAY, testObj.InjectionAttemptDelay);
      }
   }
}
