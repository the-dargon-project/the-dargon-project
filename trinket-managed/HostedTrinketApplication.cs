using Dargon.Nest.Eggs;
using Dargon.Trinkets.Hosted.Hooks;
using System;
using System.Diagnostics;
using System.Text;
using Dargon.Management.Server;
using Dargon.RADS;
using Dargon.Ryu;
using ItzWarty;
using ItzWarty.IO;
using ItzWarty.Networking;

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
      public Direct3D9HookEventPublisherNativeToBusProxy* direct3D9HookEventPublisher;
      public ulong tailCanary;

      public void Validate() {
         Trace.Assert(startCanary == kStartCanary, "startCanary == kStartCanary");
         Trace.Assert(tailCanary == kTailCanary, "tailCanary == kTailCanary");
      }

      public override string ToString() {
         return $"[{nameof(TrinketNatives)} " +
                $"{nameof(startCanary)} = 0x{startCanary.ToString("X")} ({Encoding.ASCII.GetString(BitConverter.GetBytes(startCanary))}), " +
                $"{nameof(fileHookEventPublisher)} = {(ulong)fileHookEventPublisher}, " +
                $"{nameof(direct3D9HookEventPublisher)} = {(ulong)direct3D9HookEventPublisher}, " +
                $"{nameof(tailCanary)} = 0x{tailCanary.ToString("X")} ({Encoding.ASCII.GetString(BitConverter.GetBytes(tailCanary))})]";
      }
   }

   public unsafe class HostedTrinketApplication : NestApplication {
      private const int kTrinketAirManagementPort = 21010;
      private const int kTrinketGameManagementPort = 21011;

      NestResult NestApplication.Start(HatchlingParameters parameters) {
         return Start(null);
      }

      public NestResult Start(HostedTrinketConfiguration configuration) {
         var ryu = new RyuFactory().Create();
         ryu.Touch<ItzWartyCommonsRyuPackage>();
         ryu.Touch<ItzWartyProxiesRyuPackage>();
         ryu.Set(new LeagueTrinketConfiguration {
            RadsPath = @"V:\Riot Games\League of Legends\RADS"
         });
         var role = LeagueTrinketRoleUtilities.GetRole();
         int managementPort = role == LeagueTrinketRole.Game ? kTrinketGameManagementPort :
                                 role == LeagueTrinketRole.Air ? kTrinketAirManagementPort :
                                    0;
         if (managementPort != 0) {
            var managementEndpoint = ryu.Get<ITcpEndPointFactory>().CreateLoopbackEndPoint(managementPort);
            ryu.Set<IManagementServerConfiguration>(new ManagementServerConfiguration(managementEndpoint));
         }
         ((RyuContainerImpl)ryu).Setup(true);

         var fileSystemHookEventBus = ryu.Get<FileSystemHookEventBus>();
         var fileHookEventPublisherNativeToBusProxy = new FileHookEventPublisherNativeToBusProxyFactory().Create(fileSystemHookEventBus);
         var direct3D9HookEventBus = ryu.Get<Direct3D9HookEventBus>();
         var direct3D9HookEventPublisherNativeToBusProxy = new Direct3D9HookEventPublisherNativeToBusProxyFactory().Create(direct3D9HookEventBus);

         var trinketNatives = (TrinketNatives*)configuration.TrinketNativesPointer;
         Console.WriteLine("TrinketNatives (Pre-validation): " + trinketNatives[0]);
         trinketNatives->Validate();
         trinketNatives->fileHookEventPublisher = fileHookEventPublisherNativeToBusProxy;
         trinketNatives->direct3D9HookEventPublisher = direct3D9HookEventPublisherNativeToBusProxy;
         return NestResult.Success;
      }

      public NestResult Shutdown(ShutdownReason reason) {
         return NestResult.Success;
      }
   }
}
