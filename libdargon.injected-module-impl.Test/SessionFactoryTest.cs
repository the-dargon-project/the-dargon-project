using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMockito;

namespace Dargon.InjectedModule
{
   [TestClass]
   public class SessionFactoryTest : NMockitoInstance
   {
      private SessionFactory testObj;
      
      [Mock] private readonly IDtpNodeFactory dtpNodeFactory = null;

      [TestInitialize]
      public void Setup()
      {
         InitializeMocks();

         testObj = new SessionFactory(dtpNodeFactory);
      }

      [TestMethod]
      public void TestCreateSession()
      {
         const int processId = 10;
         var configuration = CreateMock<IInjectedModuleConfiguration>();
         var dtpNode = CreateUntrackedMock<IDtpNode>();
         When(dtpNodeFactory.CreateNode(Eq(NodeRole.Server), Any<string>(), Any<IEnumerable<IInstructionSet>>())).ThenReturn(dtpNode);

         var session = testObj.CreateSession(processId, configuration);

         AssertEquals(processId, session.ProcessId);
         AssertEquals(configuration, session.Configuration);
         Verify(dtpNodeFactory).CreateNode(Eq(NodeRole.Server), Any<string>(), Any<IEnumerable<IInstructionSet>>(x => x.Count() != 0));
         VerifyNoMoreInteractions();
      }
   }
}
