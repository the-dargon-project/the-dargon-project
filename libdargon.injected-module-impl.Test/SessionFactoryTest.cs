using Dargon.Transport;
using NMockito;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Dargon.InjectedModule
{
   public class SessionFactoryTest : NMockitoInstance
   {
      private SessionFactory testObj;
      
      [Mock] private readonly IDtpNodeFactory dtpNodeFactory = null;

      public SessionFactoryTest()
      {
         testObj = new SessionFactory(dtpNodeFactory);
      }

      [Fact]
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
