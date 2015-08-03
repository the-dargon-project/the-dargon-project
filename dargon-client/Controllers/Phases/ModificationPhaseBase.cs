using Dargon.Client.ViewModels;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using ItzWarty.IO;

namespace Dargon.Client.Controllers.Phases {
   public abstract class ModificationPhaseBase : ModificationPhase {
      public abstract void HandleEnter();
      public abstract void HandleExit();

      public IPofSerializer PofSerializer { get; set; }
      public IFileSystemProxy FileSystemProxy { get; set; }
      public ModificationPhaseFactory PhaseFactory { get; set; }
      public ModificationPhaseManager PhaseManager { get; set; }
      public Modification Modification { get; set; }
      public ModificationLoader ModificationLoader { get; set; }
      public ModificationViewModel ViewModel { get; set; }
      public TemporaryFileService TemporaryFileService { get; set; }
      public LeagueBuildUtilities LeagueBuildUtilities { get; set; }
      public ExeggutorService ExeggutorService { get; set; }
   }
}