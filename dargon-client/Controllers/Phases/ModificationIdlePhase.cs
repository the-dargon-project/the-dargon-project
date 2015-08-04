using System;
using System.IO;
using Dargon.Client.ViewModels;
using Dargon.Modifications;

namespace Dargon.Client.Controllers.Phases {
   public class ModificationIdlePhase : ModificationPhaseBase {
      public override void HandleEnter() {
         var enabledComponent = Modification.GetComponent<EnabledComponent>();

         ViewModel.StatusOverride = ModificationEntryStatus.None;
         ViewModel.StatusProgress = 1;

         var thumbnailComponent = Modification.GetComponent<ThumbnailComponent>();
         thumbnailComponent.SelectThumbnailIfUnselected();
      }

      public override void HandleExit() { }
   }
}