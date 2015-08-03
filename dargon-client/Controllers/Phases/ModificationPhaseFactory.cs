using System.IO;
using System.Linq;
using Dargon.Client.ViewModels;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.Client.Controllers.Phases {
   public class ModificationPhaseFactory {
      private readonly IPofSerializer pofSerializer;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ExeggutorService exeggutorService;
      private readonly ModificationPhaseManager phaseManager;
      private readonly ModificationLoader modificationLoader;
      private readonly ModificationViewModel viewModel;
      private readonly LeagueBuildUtilities leagueBuildUtilities;
      private Modification modification;

      public ModificationPhaseFactory(IPofSerializer pofSerializer, IFileSystemProxy fileSystemProxy, TemporaryFileService temporaryFileService, ExeggutorService exeggutorService, ModificationPhaseManager phaseManager, ModificationLoader modificationLoader, ModificationViewModel viewModel, LeagueBuildUtilities leagueBuildUtilities, Modification modification) {
         this.pofSerializer = pofSerializer;
         this.fileSystemProxy = fileSystemProxy;
         this.temporaryFileService = temporaryFileService;
         this.exeggutorService = exeggutorService;
         this.phaseManager = phaseManager;
         this.modificationLoader = modificationLoader;
         this.viewModel = viewModel;
         this.leagueBuildUtilities = leagueBuildUtilities;
         this.modification = modification;
      }

      public void SetModification(Modification modification) {
         this.modification = modification;
      }

      public ModificationPhase Idle() {
         return Initialize(new ModificationIdlePhase());
      }

      public ModificationPhase Importing(string importedDirectoryPath, string[] importedFilePaths, string finalRepositoryPath) {
         importedDirectoryPath = Path.GetFullPath(importedDirectoryPath);
         importedFilePaths = importedFilePaths.Select(Path.GetFullPath).ToArray();
         var relativeImportedFilePaths = Util.Generate(importedFilePaths.Length, i => importedFilePaths[i].Substring(importedDirectoryPath.Length + 1));
         return Initialize(new ModificationImportingPhase(importedDirectoryPath, relativeImportedFilePaths, finalRepositoryPath));
      }

      private ModificationPhase Initialize(ModificationPhaseBase phase) {
         phase.PofSerializer = pofSerializer;
         phase.FileSystemProxy = fileSystemProxy;
         phase.PhaseFactory = this;
         phase.PhaseManager = phaseManager;
         phase.Modification = modification;
         phase.ModificationLoader = modificationLoader;
         phase.ViewModel = viewModel;
         phase.TemporaryFileService = temporaryFileService;
         phase.ExeggutorService = exeggutorService;
         phase.LeagueBuildUtilities = leagueBuildUtilities;
         return phase;
      }
   }
}