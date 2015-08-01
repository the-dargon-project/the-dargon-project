using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Client.Controllers.Phases;
using Dargon.Client.ViewModels;
using Dargon.Modifications;

namespace Dargon.Client.Controllers {
   public class ModificationController {
      private readonly ModificationViewModel viewModel;
      private Modification modification;
      private ModificationPhaseManager modificationPhaseManager;

      public ModificationController(Modification modification, ModificationViewModel viewModel) {
         this.modification = modification;
         this.viewModel = viewModel;
      }

      public void Initialize() {

      }

      public void SetModificationPhaseManager(ModificationPhaseManager modificationPhaseManager) {
         this.modificationPhaseManager = modificationPhaseManager;
      }
   }
}
