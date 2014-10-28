using System;
using System.Text.RegularExpressions;
using ItzWarty;

namespace Dargon
{
   public class VersionStringUtilities
   {
      public string GetVersionString(uint n)
      {
         var b0 = n & 0xFF;
         var b1 = (n >> 8) & 0xFF;
         var b2 = (n >> 16) & 0xFF;
         var b3 = (n >> 24) & 0xFF;
         return b3 + "." + b2 + "." + b1 + "." + b0;
      }

      /// <summary>
      /// Gets the version string from the given path.
      /// If no match is found
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public string GetVersionString(string s)
      {
         var matchResult = Regex.Match(s, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
         if (matchResult.Success) return matchResult.Value;
         else return "";
      }

      public uint GetVersionNumber(string s)
      {
         var parts = GetVersionString(s).Split(".");
         if (parts.Length != 4)
            return uint.MaxValue;
         uint result = 0;
         for (int i = 0; i < 4; i++)
            result = (result << 8) | UInt32.Parse(parts[i]);
         return result;
      }

      public bool TryGetVersionNumber(string s, out uint versionNumber)
      {
         var match = Regex.Match(s, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
         if (!match.Success) {
            versionNumber = uint.MaxValue;
            return false;
         } else {
            var parts = match.Value.Split(".");
            uint result = 0;
            for (int i = 0; i < 4; i++)
               result = (result << 8) | UInt32.Parse(parts[i]);
            versionNumber = result;
            return true;
         }
      }
   }
}
