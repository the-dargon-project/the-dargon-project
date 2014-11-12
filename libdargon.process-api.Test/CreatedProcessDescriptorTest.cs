using Dargon.Processes.Watching;
using NMockito;
using Xunit;

namespace Dargon.Processes
{
   public class CreatedProcessDescriptorTest : NMockitoInstance
   {
      private CreatedProcessDescriptor testObj;
      private const string PROCESS_NAME = "AIODJI(O@!JNMFA.OIXJO(#I";
      private const int PROCESS_ID = 129042;
      private const int PARENT_PROCESS_ID = 28793298;

      public CreatedProcessDescriptorTest()
      {
         testObj = new CreatedProcessDescriptor(PROCESS_NAME, PROCESS_ID, PARENT_PROCESS_ID);
      }

      [Fact]
      public void PropertiesReflectConstructorArgumentsTest()
      {
         AssertEquals(PROCESS_NAME, testObj.ProcessName);
         AssertEquals(PROCESS_ID, testObj.ProcessId);
         AssertEquals(PARENT_PROCESS_ID, testObj.ParentProcessId);
      }
   }
}
