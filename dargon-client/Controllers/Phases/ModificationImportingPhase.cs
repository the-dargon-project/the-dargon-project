using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dargon.Client.Controllers.Helpers;
using Dargon.Client.ViewModels;
using Dargon.Modifications;
using Dargon.Modifications.ThumbnailGenerator;
using Dargon.Nest.Eggxecutor;
using ItzWarty;
using NLog;

namespace Dargon.Client.Controllers.Phases {
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
         Modification = ModificationLoader.FromPath(finalRepositoryPath);
         PhaseFactory.SetModification(Modification);
         ViewModel.SetModification(Modification);

         var thumbnailDirectory = Path.Combine(finalRepositoryPath, "thumbnails");
         FileSystemProxy.PrepareDirectory(thumbnailDirectory);
         var thumbnailGenerationTask = Task.Factory.StartNew(() => {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms)) {
               PofSerializer.Serialize(writer, new ThumbnailGenerationParameters {
                  DestinationDirectory = thumbnailDirectory,
                  SourceDirectory = importedDirectoryPath,
                  ThumbnailsToGenerate = 3
               });
               ExeggutorService.SpawnHatchling(
                  "thumbnail-generator",
                  new SpawnConfiguration {
                     InstanceName = "thumbnail-generator-" + DateTime.UtcNow.GetUnixTime(),
                     Arguments = ms.GetBuffer()
                  });
               var thumbnailComponent = Modification.GetComponent<ThumbnailComponent>();
               thumbnailComponent.SelectThumbnailIfUnselected();
            }
         }, TaskCreationOptions.LongRunning);

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
         thumbnailGenerationTask.Wait();
         PhaseManager.Transition(PhaseFactory.Idle());
      }

      private void UpdateProgress(double percentage) {
         Application.Current.Dispatcher.BeginInvoke(new Action(() => {
            ViewModel.StatusProgress = percentage;
         }), DispatcherPriority.Send);
      }

      public override void HandleExit() { }
   }
}
