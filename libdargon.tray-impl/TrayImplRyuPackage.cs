using Dargon.Ryu;

namespace Dargon.Tray {
   public class TrayImplRyuPackage : RyuPackageV1 {
      public TrayImplRyuPackage() {
         Singleton<TrayServiceImpl>();
         Singleton<TrayService, TrayServiceImpl>();
      }
   }
}
