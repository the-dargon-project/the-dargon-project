using Dargon.Nest.Eggs;
using Dargon.Trinkets.Hosted;
using ItzWarty;
using System;

/// <summary>
/// Do not rename or place in namespace!
/// Must have FullName "TrinketEntryPoint" with
/// TrinketMain = func(string) -> int
/// </summary>
// ReSharper disable once CheckNamespace
public static class TrinketEntryPoint {
   public static int TrinketMain(string args) {
      try {
         Console.WriteLine($"Entered {nameof(TrinketMain)}");
         var configuration = ParseConfiguration(args);
         var egg = new HostedTrinketApplication();
         var result = egg.Start(configuration);
         return result == NestResult.Success ? 0 : 1;
      } catch (Exception e) {
         Console.Error.WriteLine(e);
         return 2;
      }
   }

   private static HostedTrinketConfiguration ParseConfiguration(string args) {
      Console.WriteLine("Parsing configuration args: " + args);
      var initialArgs = string.Copy(args);
      string trinketNativesPointerIntString;
      args = Util.NextToken(args, out trinketNativesPointerIntString);
      return new HostedTrinketConfiguration {
         Args = initialArgs,
         TrinketNativesPointer = (IntPtr)Int64.Parse(trinketNativesPointerIntString)
      };
   }
}
