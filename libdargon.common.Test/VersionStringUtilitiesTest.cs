using Dargon;
using NMockito;
using Xunit;

namespace libdargon.common.Test
{
   public class VersionStringUtilitiesTest : NMockitoInstance
   {
      private VersionStringUtilities testObj = new VersionStringUtilities();

      private const string kInvalidPath = "123ids890/A()!K/!)(DKQ:";
      private const string kValidPath = "123ids890/1.2.3.4/!)(DKQ:";
      private const string kValidPathVersionString = "1.2.3.4";
      private const uint kValidPathVersionNumber = 0x01020304U;

      [Fact]
      public void GetVersionNumberHappyPathTest() { AssertEquals(kValidPathVersionNumber, testObj.GetVersionNumber(kValidPath)); }

      [Fact]
      public void GetVersionNumberSadPathReturnsUint32MaxTest() { AssertEquals(uint.MaxValue, testObj.GetVersionNumber(kInvalidPath)); }

      [Fact]
      public void GetVersionStringHappyPathTest() { AssertEquals(kValidPathVersionString, testObj.GetVersionString(kValidPath)); }

      [Fact]
      public void GetVersionStringInvalidStringReturnsEmptyStringTest() { AssertEquals("", testObj.GetVersionString(kInvalidPath)); }

      [Fact]
      public void TryGetVersionNumberHappyPathTest()
      {
         uint versionNumber;
         AssertTrue(testObj.TryGetVersionNumber(kValidPath, out versionNumber));
         AssertEquals(kValidPathVersionNumber, versionNumber);
      }

      [Fact]
      public void TryGetVersionNumberSadPathTest()
      {
         uint versionNumber;
         AssertFalse(testObj.TryGetVersionNumber(kInvalidPath, out versionNumber));
         AssertEquals(uint.MaxValue, versionNumber);
      }
   }
}
