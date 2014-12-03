using System.Windows.Forms.Integration;
using Dargon.Daemon;
using Dargon.Manager.Controllers;
using Dargon.ModificationRepositories;
using Dargon.Services.Client;
using DargonManager;

namespace Dargon.Manager {
   public class DargonManagerApplication {
      private readonly DaemonService daemonService;
      private readonly ModificationRepositoryService modificationRepositoryService;

      public DargonManagerApplication(IServiceClient serviceClient) {
         daemonService = serviceClient.GetService<DaemonService>();
         modificationRepositoryService = serviceClient.GetService<ModificationRepositoryService>();
      }

      public void Run() {
         var rootController = new RootController(daemonService, modificationRepositoryService);

         var mainWindow = new MainWindow(rootController);
         ElementHost.EnableModelessKeyboardInterop(mainWindow); // Makes it so that we can type in textboxes...
         mainWindow.Show();
      }
   }
}
