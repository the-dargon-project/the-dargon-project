using System;
using System.IO;
using Dargon.Client.ViewModels;
using Dargon.Modifications;

namespace Dargon.Client.Controllers.Phases {
   public class ModificationIdlePhase : ModificationPhaseBase {
      public override void HandleEnter() {
         ViewModel.Status = ModificationStatus.Enabled;
         ViewModel.StatusProgress = 1;

         var thumbnailComponent = Modification.GetComponent<ThumbnailComponent>();
         thumbnailComponent.SelectThumbnailIfUnselected();
      }

      public override void HandleExit() { }
   }
}