using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Dargon.Daemon;
using Dargon.Manager.Models;
using Dargon.Manager.ViewModels;

namespace Dargon.Manager.Controllers {
   public class RootController {
      private readonly DaemonService daemonService;
      private readonly StatusController statusController;
      private readonly ModificationListingViewModel modificationListingViewModel;
      private readonly ModificationImportController modificationImportController;

      public RootController(DaemonService daemonService, StatusController statusController, ModificationListingViewModel modificationListingViewModel, ModificationImportController modificationImportController) {
         this.daemonService = daemonService;
         this.statusController = statusController;
         this.modificationImportController = modificationImportController;
         this.modificationListingViewModel = modificationListingViewModel;
      }

      public void SignalDaemonShutdown() {
         daemonService.Shutdown();
      }

      public StatusModel GetStatusModel() {
         return statusController.Model;
      }

      public ModificationListingViewModel GetModificationListingViewModel() {
         return modificationListingViewModel;
      }

      public ModificationImportController GetModificationImportController() {
         return modificationImportController;
      }
   }

   public class StatusController {
      private readonly StatusModelImpl statusModel;

      public StatusController(StatusModelImpl statusModel) {
         this.statusModel = statusModel;
      }

      public StatusModel Model => statusModel;
      
      public void Update(string status) {
         statusModel.Status = status;
      }
   }
}
