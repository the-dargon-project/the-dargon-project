using System.Diagnostics;
using System.IO;

namespace Dargon.InjectedModule
{
   public class InjectedModuleServiceConfiguration : IInjectedModuleServiceConfiguration
   {
      public string GetInjectedDllPath()
      {
#if DEBUG
         var driveName = new DriveInfo(Process.GetCurrentProcess().Modules[0].FileName).Name;
         return Path.Combine(driveName, @"my-repositories\the-dargon-project\Debug", "Dargon - Injected Module.dll");
#else
   // TODO: Manifest for DIM.dll
#endif
      }
   }
}