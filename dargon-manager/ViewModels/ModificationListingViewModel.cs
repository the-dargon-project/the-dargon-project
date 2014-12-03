using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Game;
using Dargon.Manager.Models;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.PortableObjects;

namespace Dargon.Manager.ViewModels {
   public class ModificationListingViewModel {
      private readonly ImportValidityModel importValidityModel;
      private readonly ObservableCollection<ModificationViewModel> items;

      public ModificationListingViewModel(ImportValidityModel importValidityModel, ObservableCollection<ModificationViewModel> items) {
         this.importValidityModel = importValidityModel;
         this.items = items;
      }

      public ImportValidityModel ImportValidityModel { get { return importValidityModel; } }

      public ObservableCollection<ModificationViewModel> Items { get { return items; } }
   }
}
