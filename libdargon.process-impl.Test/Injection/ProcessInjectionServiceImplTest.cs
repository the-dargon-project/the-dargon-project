using System;
using ItzWarty.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMockito;

namespace Dargon.Processes.Injection
{
   [TestClass]
   public class ProcessInjectionServiceImplTest : NMockitoInstance
   {
      private ProcessInjectionServiceImpl testObj;

      [Mock] private readonly IServiceLocator serviceLocator = null;
      [Mock] private readonly IProcessInjector processInjector = null;

      [TestInitialize]
      public void Setup()
      {
         InitializeMocks();

         testObj = new ProcessInjectionServiceImpl(serviceLocator, processInjector);

         Verify(serviceLocator).RegisterService(typeof(ProcessInjectionService), testObj);
         VerifyNoMoreInteractions();
      }

      [TestMethod]
      public void InjectToProcessDelegatesToProcessInjectorTest()
      {
         const int processId = 123;
         const string dllPath = "path";
         const int attempts = 5932;
         const int attemptDelay = 193042;

         When(processInjector.TryInject(processId, dllPath, attempts, attemptDelay)).ThenReturn(true, false);

         AssertTrue(testObj.InjectToProcess(processId, dllPath, attempts, attemptDelay));
         AssertFalse(testObj.InjectToProcess(processId, dllPath, attempts, attemptDelay));

         Verify(processInjector, Times(2)).TryInject(processId, dllPath, attempts, attemptDelay);
         VerifyNoMoreInteractions();
      }
   }
}
