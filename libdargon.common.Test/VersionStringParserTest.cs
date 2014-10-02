using System;
using Dargon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMockito;

namespace libdargon.common.Test
{
   [TestClass]
   public class VersionStringParserTest : NMockitoInstance
   {
      private VersionStringParser testObj = new VersionStringParser();

      private const string kInvalidPath = "123ids890/A()!K/!)(DKQ:";
      private const string kValidPath = "123ids890/1.2.3.4/!)(DKQ:";
      private const string kValidPathVersionString = "1.2.3.4";
      private const uint kValidPathVersionNumber = 0x01020304U;

      [TestMethod]
      public void GetVersionNumberHappyPathTest() { AssertEquals(kValidPathVersionNumber, testObj.GetVersionNumber(kValidPath)); }

      [TestMethod]
      public void GetVersionNumberSadPathReturnsUint32MaxTest() { AssertEquals(uint.MaxValue, testObj.GetVersionNumber(kInvalidPath)); }

      [TestMethod]
      public void GetVersionStringHappyPathTest() { AssertEquals(kValidPathVersionString, testObj.GetVersionString(kValidPath)); }
      
      [TestMethod]
      public void GetVersionStringInvalidStringReturnsEmptyStringTest() { AssertEquals("", testObj.GetVersionString(kInvalidPath)); }

      [TestMethod]
      public void TryGetVersionNumberHappyPathTest()
      {
         uint versionNumber;
         AssertTrue(testObj.TryGetVersionNumber(kValidPath, out versionNumber));
         AssertEquals(kValidPathVersionNumber, versionNumber);
      }

      [TestMethod]
      public void TryGetVersionNumberSadPathTest()
      {
         uint versionNumber;
         AssertFalse(testObj.TryGetVersionNumber(kInvalidPath, out versionNumber));
         AssertEquals(uint.MaxValue, versionNumber);
      }
   }
}
