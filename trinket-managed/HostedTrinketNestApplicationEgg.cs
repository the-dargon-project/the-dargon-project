using Dargon.Nest.Egg;
using System;
using System.Diagnostics;
using System.Text;
using Dargon.Trinkets.Hosted.Hooks;

namespace Dargon.Trinkets.Hosted {
   public class HostedTrinketConfiguration {
      public string Args { get; set; }
      public IntPtr TrinketNativesPointer { get; set; }
   }

   public unsafe struct TrinketNatives {
      public const ulong kStartCanary = 0x74656b6e69727464U; // dtrinket
      public const ulong kTailCanary = 0x54454b4e49525444U; // DTRINKET

      public ulong startCanary;
      public FileHookEventPublisherNativeToBusProxy* fileHookEventPublisher;
      public ulong tailCanary;

      public void Validate() {
         Trace.Assert(startCanary == kStartCanary, "startCanary == kStartCanary");
         Trace.Assert(tailCanary == kTailCanary, "tailCanary == kTailCanary");
      }

      public override string ToString() {
         return $"[{nameof(TrinketNatives)} " +
                $"{nameof(startCanary)} = 0x{startCanary.ToString("X")} ({Encoding.ASCII.GetString(BitConverter.GetBytes(startCanary))}), " +
                $"{nameof(fileHookEventPublisher)} = {(ulong)fileHookEventPublisher}, " +
                $"{nameof(tailCanary)} = 0x{tailCanary.ToString("X")} ({Encoding.ASCII.GetString(BitConverter.GetBytes(tailCanary))})]";
      }
   }

   public unsafe class HostedTrinketNestApplicationEgg : INestApplicationEgg {
      NestResult INestApplicationEgg.Start(IEggParameters parameters) {
         return Start(null);
      }

      public NestResult Start(HostedTrinketConfiguration configuration) {
         var fileSystemHookEventBus = new FileSystemHookEventBusImpl();
         fileSystemHookEventBus.CreateFilePre += (s, e) => {
            Console.WriteLine("CreateFile: " + e.Arguments.Path + " " + e.Arguments.Access);
         };
         var proxyFactory = new FileHookEventPublisherNativeToBusProxyFactory();
         var fileHookEventPublisherNativeToBusProxy = proxyFactory.Create(fileSystemHookEventBus);
         
         var trinketNatives = (TrinketNatives*)configuration.TrinketNativesPointer;
         Console.WriteLine("TrinketNatives (Pre-validation): " + trinketNatives[0]);
         trinketNatives->Validate();
         trinketNatives->fileHookEventPublisher = fileHookEventPublisherNativeToBusProxy;
         return NestResult.Success;
      }

      public NestResult Shutdown(ShutdownReason reason) {
         return NestResult.Success;
      }
   }
}
