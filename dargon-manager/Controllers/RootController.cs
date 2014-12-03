using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Dargon.Daemon;
using Dargon.Manager.Models;
using Dargon.Manager.ViewModels;
using Dargon.ModificationRepositories;

namespace Dargon.Manager.Controllers {
   public class RootController {
      private readonly DaemonService daemonService;
      private readonly ModificationRepositoryController modificationRepositoryController;
      private readonly ImportValidityModelImpl importValidityModel;
      private readonly StatusModelImpl statusModel;
      private readonly StatusController statusController;
      private readonly ModificationListingViewModel modificationListingViewModel;
      private readonly ModificationImportController modificationImportController;
      private readonly ModificationRepositoryService modificationRepositoryService;

      public RootController(DaemonService daemonService, ModificationRepositoryService modificationRepositoryService) {
         this.daemonService = daemonService;
         this.modificationRepositoryService = modificationRepositoryService;
         this.importValidityModel = new ImportValidityModelImpl();
         this.statusModel = new StatusModelImpl();
         this.statusController = new StatusController(statusModel);
         this.modificationRepositoryController = new ModificationRepositoryController(modificationRepositoryService, modificationListingViewModel);
         this.modificationListingViewModel = new ModificationListingViewModel(importValidityModel, modificationRepositoryController.GetSynchronizedLocalRepositoryModifications());
         this.modificationImportController = new ModificationImportController(statusController, importValidityModel);
      }

      public void SignalDaemonShutdown() {
         daemonService.Shutdown();
      }

      public StatusModel GetStatusModel() {
         return statusModel;
      }

      public ModificationListingViewModel GetModificationListingViewModel() {
         return modificationListingViewModel;
      }

      public ModificationRepositoryController GetModificationRepositoryController() {
         return modificationRepositoryController;
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

      public void Update(string status) {
         statusModel.Status = status;
      }
   }
}
