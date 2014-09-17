using System.Threading;
using System.Windows.Forms;
using Dargon.Daemon;
using Dargon.Properties;
using ItzWarty.Services;

namespace Dargon.Tray
{
   public class TrayServiceImpl : TrayService
   {
      private readonly DaemonService daemonService;
      private NotifyIcon notifyIcon;

      public TrayServiceImpl(IServiceLocator serviceLocator, DaemonService daemonService) 
      {
         this.daemonService = daemonService;

         serviceLocator.RegisterService(typeof(TrayService), this);
         new Thread(TrayServiceThreadStart) { IsBackground = true }.Start();
      }

      private void TrayServiceThreadStart()
      {
         notifyIcon = new NotifyIcon();
         notifyIcon.Icon = CommonResources.Resources2011.Icon;
         notifyIcon.Visible = true;

         var menu = new ContextMenu();

         menu.MenuItems.Add(
            "Quit",
            (s, e) => daemonService.Shutdown());

         notifyIcon.ContextMenu = menu;

         daemonService.BeginShutdown += (sender, args) => notifyIcon.Visible = false;
         Application.Run();
      }
   }
}
