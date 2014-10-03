using Dargon.Processes.Watching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMockito;

namespace Dargon.Processes
{
   [TestClass]
   public class CreatedProcessDescriptorTest : NMockitoInstance
   {
      private CreatedProcessDescriptor testObj;
      private const string PROCESS_NAME = "AIODJI(O@!JNMFA.OIXJO(#I";
      private const int PROCESS_ID = 129042;
      private const int PARENT_PROCESS_ID = 28793298;

      [TestInitialize]
      public void Setup()
      {
         testObj = new CreatedProcessDescriptor(PROCESS_NAME, PROCESS_ID, PARENT_PROCESS_ID);
      }

      [TestMethod]
      public void PropertiesReflectConstructorArgumentsTest()
      {
         AssertEquals(PROCESS_NAME, testObj.ProcessName);
         AssertEquals(PROCESS_ID, testObj.ProcessId);
         AssertEquals(PARENT_PROCESS_ID, testObj.ParentProcessId);
      }
   }
}
