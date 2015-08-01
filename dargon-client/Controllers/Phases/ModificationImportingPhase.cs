using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dargon.Client.Controllers.Helpers;
using Dargon.Client.ViewModels;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Modifications;
using ItzWarty;
using ItzWarty.IO;
using NLog;

namespace Dargon.Client.Controllers.Phases {
   public interface ModificationPhase {
      void HandleEnter();
      void HandleExit();
   }

   public class ModificationPhaseManager {
      private readonly object synchronization = new object();
      private ModificationPhase currentPhase = null;

      public void Transition(ModificationPhase phase) {
         lock (synchronization) {
            currentPhase?.HandleExit();
            currentPhase = phase;
            currentPhase.HandleEnter();
         }
      }
   }

   public class ModificationPhaseFactory {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ModificationPhaseManager phaseManager;
      private readonly ModificationLoader modificationLoader;
      private readonly ModificationViewModel viewModel;
      private readonly LeagueBuildUtilities leagueBuildUtilities;
      private Modification modification;

      public ModificationPhaseFactory(IFileSystemProxy fileSystemProxy, TemporaryFileService temporaryFileService, ModificationPhaseManager phaseManager, ModificationLoader modificationLoader, ModificationViewModel viewModel, LeagueBuildUtilities leagueBuildUtilities, Modification modification) {
         this.fileSystemProxy = fileSystemProxy;
         this.temporaryFileService = temporaryFileService;
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
         phase.FileSystemProxy = fileSystemProxy;
         phase.PhaseFactory = this;
         phase.PhaseManager = phaseManager;
         phase.Modification = modification;
         phase.ModificationLoader = modificationLoader;
         phase.ViewModel = viewModel;
         phase.TemporaryFileService = temporaryFileService;
         phase.LeagueBuildUtilities = leagueBuildUtilities;
         return phase;
      }
   }

   public abstract class ModificationPhaseBase : ModificationPhase {
      public abstract void HandleEnter();
      public abstract void HandleExit();

      public IFileSystemProxy FileSystemProxy { get; set; }
      public ModificationPhaseFactory PhaseFactory { get; set; }
      public ModificationPhaseManager PhaseManager { get; set; }
      public Modification Modification { get; set; }
      public ModificationLoader ModificationLoader { get; set; }
      public ModificationViewModel ViewModel { get; set; }
      public TemporaryFileService TemporaryFileService { get; set; }
      public LeagueBuildUtilities LeagueBuildUtilities { get; set; }
   }

   public class ModificationImportingPhase : ModificationPhaseBase {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly string importedDirectoryPath;
      private readonly string[] relativeImportedFilePaths;
      private readonly string finalRepositoryPath;

      public ModificationImportingPhase(string importedDirectoryPath, string[] relativeImportedFilePaths, string finalRepositoryPath) {
         this.importedDirectoryPath = importedDirectoryPath;
         this.relativeImportedFilePaths = relativeImportedFilePaths;
         this.finalRepositoryPath = finalRepositoryPath;
      }

      public override void HandleEnter() {
         ViewModel.Status = ModificationStatus.Updating;
         ViewModel.StatusProgress = 0;
         Task.Factory.StartNew(new Action(() => {
            try {
               ImportingTaskStart();
            } catch (Exception e) {
               logger.Error(e);
            }
         }), TaskCreationOptions.LongRunning);
      }

      public void ImportingTaskStart() {
         var temporaryDirectory = TemporaryFileService.AllocateTemporaryDirectory(TimeSpan.FromHours(1));
         DirectoryHelpers.DirectoryCopy(Modification.RepositoryPath, temporaryDirectory, true);
         Directory.Move(temporaryDirectory, finalRepositoryPath);
         PhaseFactory.SetModification(Modification = ModificationLoader.FromPath(finalRepositoryPath));

         var contentDirectory = Path.Combine(finalRepositoryPath, "content");
         FileSystemProxy.PrepareDirectory(contentDirectory);
         for (var i = 0; i < relativeImportedFilePaths.Length; i++) {
            var sourceFile = Path.Combine(importedDirectoryPath, relativeImportedFilePaths[i]);
            var destinationFile = Path.Combine(contentDirectory, relativeImportedFilePaths[i]);
            FileSystemProxy.PrepareParentDirectory(destinationFile);
            FileSystemProxy.CopyFile(sourceFile, destinationFile);
            UpdateProgress(0.333 *((double)i / relativeImportedFilePaths.Length));
         }

         LeagueBuildUtilities.ResolveModification(Modification, CancellationToken.None);
         UpdateProgress(0.666);

         LeagueBuildUtilities.CompileModification(Modification, CancellationToken.None);
         UpdateProgress(1);
         PhaseManager.Transition(PhaseFactory.Idle());
      }

      private void UpdateProgress(double percentage) {
         Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            ViewModel.StatusProgress = percentage;
         }), DispatcherPriority.Send);
      }

      public override void HandleExit() { }
   }

   public class ModificationIdlePhase : ModificationPhaseBase {
      public override void HandleEnter() {
         ViewModel.Status = ModificationStatus.Enabled;
         ViewModel.StatusProgress = 1;
      }

      public override void HandleExit() { }
   }
}
