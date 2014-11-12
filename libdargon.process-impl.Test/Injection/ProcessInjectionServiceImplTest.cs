using System;
using ItzWarty;
using ItzWarty.Services;
using NMockito;
using Xunit;

namespace Dargon.Processes.Injection
{
   public class ProcessInjectionServiceImplTest : NMockitoInstance
   {
      private ProcessInjectionServiceImpl testObj;

      [Mock] private readonly IServiceLocator serviceLocator = null;
      [Mock] private readonly IProcessInjector processInjector = null;
      [Mock] private readonly IProcessInjectionConfiguration processInjectionConfiguration;

      public ProcessInjectionServiceImplTest()
      {
         testObj = new ProcessInjectionServiceImpl(serviceLocator, processInjector, processInjectionConfiguration);

         Verify(serviceLocator).RegisterService(typeof(ProcessInjectionService), testObj);
         VerifyNoMoreInteractions();
      }

      [Fact]
      public void InjectToProcessDelegatesToProcessInjectorTest()
      {
         const int processId = 123;
         const string dllPath = "path";
         const int attempts = 5932;
         const int attemptDelay = 193042;

         When(processInjectionConfiguration.InjectionAttempts).ThenReturn(attempts);
         When(processInjectionConfiguration.InjectionAttemptDelay).ThenReturn(attemptDelay);
         When(processInjector.TryInject(processId, dllPath, attempts, attemptDelay)).ThenReturn(true, false);

         AssertTrue(testObj.InjectToProcess(processId, dllPath));
         AssertFalse(testObj.InjectToProcess(processId, dllPath));

         Verify(processInjectionConfiguration, Times(2)).InjectionAttempts.Wrap();
         Verify(processInjectionConfiguration, Times(2)).InjectionAttemptDelay.Wrap();
         Verify(processInjector, Times(2)).TryInject(processId, dllPath, attempts, attemptDelay);
         VerifyNoMoreInteractions();
      }
   }
}
