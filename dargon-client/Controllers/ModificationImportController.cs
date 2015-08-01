using Dargon.Client.ViewModels;
using Dargon.Client.Views;
using Dargon.IO;
using Dargon.IO.Resolution;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.RADS;
using ItzWarty;
using ItzWarty.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dargon.Client.Controllers.Phases;
using Dargon.Modifications;
using Dargon.Nest;
using Dargon.Nest.Eggxecutor;
using Dargon.Patcher;
using Dargon.PortableObjects;

namespace Dargon.Client.Controllers {
   public class ModificationImportController {
      private readonly IPofSerializer pofSerializer;
      private readonly string repositoriesDirectory;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ExeggutorService exeggutorService;
      private readonly ModificationComponentFactory modificationComponentFactory;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly RiotSolutionLoader riotSolutionLoader;
      private readonly ModificationImportViewModelFactory modificationImportViewModelFactory;
      private readonly ObservableCollection<ModificationViewModel> modificationViewModels;
      private readonly ModificationLoader modificationLoader;
      private readonly LeagueBuildUtilities leagueBuildUtilities;

      public ModificationImportController(IPofSerializer pofSerializer, string repositoriesDirectory, TemporaryFileService temporaryFileService, ExeggutorService exeggutorService, ModificationComponentFactory modificationComponentFactory, IFileSystemProxy fileSystemProxy, RiotSolutionLoader riotSolutionLoader, ModificationImportViewModelFactory modificationImportViewModelFactory, ObservableCollection<ModificationViewModel> modificationViewModels, ModificationLoader modificationLoader, LeagueBuildUtilities leagueBuildUtilities) {
         this.pofSerializer = pofSerializer;
         this.repositoriesDirectory = repositoriesDirectory;
         this.temporaryFileService = temporaryFileService;
         this.exeggutorService = exeggutorService;
         this.modificationComponentFactory = modificationComponentFactory;
         this.fileSystemProxy = fileSystemProxy;
         this.riotSolutionLoader = riotSolutionLoader;
         this.modificationImportViewModelFactory = modificationImportViewModelFactory;
         this.modificationViewModels = modificationViewModels;
         this.modificationLoader = modificationLoader;
         this.leagueBuildUtilities = leagueBuildUtilities;
      }

      public void ShowModificationImportWindowDialog(string modificationPath) {
         var rootNodeViewModel = modificationImportViewModelFactory.FromDirectory(modificationPath);
         var solution = riotSolutionLoader.Load(@"V:\Riot Games\League of Legends\RADS", RiotProjectType.AirClient | RiotProjectType.GameClient);
         var airResolver = new Resolver(solution.ProjectsByType[RiotProjectType.AirClient].ReleaseManifest.Root);
         var gameResolver = new Resolver(solution.ProjectsByType[RiotProjectType.GameClient].ReleaseManifest.Root);

         var fileNodes = rootNodeViewModel.EnumerateFileNodes().ToArray();
         var importWindow = new ModificationImportWindow();
         var modificationImportViewModel = new ModificationImportViewModel(this, importWindow, rootNodeViewModel);
         modificationImportViewModel.ModificationFriendlyName = fileSystemProxy.GetDirectoryInfo(modificationPath).Name;
         importWindow.DataContext = modificationImportViewModel;
         new Thread(() => {
            foreach (var fileNode in fileNodes) {
               var path = fileNode.Path;
               var airResolution = airResolver.Resolve(path);
               if (airResolution.Any()) {
                  fileNode.ResolutionPath = airResolution.First().GetPath();
                  fileNode.ResolutionState = ResolutionState.ResolutionSuccessful;
               } else {
                  var gameResolutions = gameResolver.Resolve(path);
                  if (gameResolutions.Any()) {
                     fileNode.ResolutionPath = gameResolutions.First().GetPath();
                     fileNode.ResolutionState = ResolutionState.ResolutionSuccessful;
                  } else {
                     fileNode.ResolutionState = ResolutionState.ResolutionFailed;
                  }
               }
            }

            LeagueModificationCategory modificationType = LeagueModificationCategory.Other;
            if (fileNodes.Any(node => node.ResolutionState == ResolutionState.ResolutionSuccessful)) {
               var modificationTypeCounts = new ConcurrentDictionary<LeagueModificationCategory, int>();
               foreach (var file in fileNodes) {
                  if (file.ResolutionState == ResolutionState.ResolutionSuccessful) {
                     if (file.ResolutionPath.IndexOf("DATA/Characters", StringComparison.OrdinalIgnoreCase) != -1 ||
                         file.ResolutionPath.IndexOf("assets/images/champions", StringComparison.OrdinalIgnoreCase) != -1) {
                        if (file.ResolutionPath.IndexOf("ward", StringComparison.OrdinalIgnoreCase) != -1) {
                           modificationTypeCounts.AddOrUpdate(LeagueModificationCategory.Ward, 1, (existing, count) => count + 1);
                        } else {
                           modificationTypeCounts.AddOrUpdate(LeagueModificationCategory.Champion, 1, (existing, count) => count + 1);
                        }
                     } else if (file.ResolutionPath.IndexOf("LEVELS") != -1) {
                        modificationTypeCounts.AddOrUpdate(LeagueModificationCategory.Map, 1, (existing, count) => count + 1);
                     } else if (file.ResolutionPath.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) != -1) {
                        modificationTypeCounts.AddOrUpdate(LeagueModificationCategory.UserInterface, 1, (existing, count) => count + 1);
                     } else {
                        modificationTypeCounts.AddOrUpdate(LeagueModificationCategory.Other, 1, (existing, count) => count + 1);
                     }
                  }
               }
               var categorizationCounts = modificationTypeCounts.Sum(x => x.Value);
               var highestCategorization = modificationTypeCounts.MaxBy(key => key.Value, Comparer<int>.Default);
               if (highestCategorization.Value >= categorizationCounts * 2.0 / 3.0) {
                  modificationType = modificationTypeCounts.MaxBy(key => key.Value, Comparer<int>.Default).Key;
               }
               Console.WriteLine("Highest categorization: " + highestCategorization.Key.Name);
               modificationTypeCounts.ForEach(x => Console.WriteLine(x.Key.Name + ": " + x.Value));
               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                  modificationImportViewModel.ModificationCategorization = modificationType;
               }));
            }
         }).Start();
         importWindow.ShowDialog();
      }

      public void ImportLegacyModification(string friendlyModificationName, string importedDirectoryPath, string[] importedFilePaths, LeagueModificationCategory category, ModificationImportWindow importWindow) {
         string repositoryName = Util.ExtractFileNameTokens(friendlyModificationName).Select(token => token.ToLower()).Join("-");
         string finalRepositoryPath = Path.Combine(repositoriesDirectory, repositoryName);
         var temporaryDirectory = temporaryFileService.AllocateTemporaryDirectory(TimeSpan.FromHours(1));
         var workingDirectory = Path.Combine(temporaryDirectory, "working");
         var contentDirectory = Path.Combine(workingDirectory, "content");
         var temporaryNestPath = Path.Combine(temporaryDirectory, "working_nest");
         fileSystemProxy.PrepareDirectory(contentDirectory);
         fileSystemProxy.PrepareDirectory(temporaryNestPath);

         var temporaryNest = new LocalDargonNest(temporaryNestPath);
         var inMemoryEgg = new InMemoryEgg(repositoryName, "legacy", workingDirectory);
         temporaryNest.InstallEgg(inMemoryEgg);

         var temporaryEggPath = Path.Combine(temporaryNestPath, inMemoryEgg.Name);

         var temporaryModification = modificationLoader.FromPath(temporaryEggPath);
         var info = temporaryModification.GetComponent<InfoComponent>();
         info.Id = Guid.NewGuid();
         info.Name = friendlyModificationName;

         var leagueComponent = temporaryModification.GetComponent<LeagueMetadataComponent>();
         leagueComponent.Category = category;

         var viewModel = new ModificationViewModel();
         var controller = new ModificationController(null, viewModel);
         viewModel.SetController(controller);
         viewModel.SetModification(temporaryModification);

         Application.Current.Dispatcher.BeginInvoke(
            new Action(() => {
               modificationViewModels.Add(viewModel);

               importWindow.Close();

               var modificationPhaseManager = new ModificationPhaseManager();
               var modificationPhaseFactory = new ModificationPhaseFactory(pofSerializer, fileSystemProxy, temporaryFileService, exeggutorService, modificationPhaseManager, modificationLoader, viewModel, leagueBuildUtilities, temporaryModification);
               controller.SetModificationPhaseManager(modificationPhaseManager);
               modificationPhaseManager.Transition(modificationPhaseFactory.Importing(importedDirectoryPath, importedFilePaths, finalRepositoryPath));
               controller.Initialize();
            }), DispatcherPriority.Send
         );

         //         var modification = modificationLoader.FromPath(e.FullPath);

         //         fileSystemProxy.MoveDirectory(temporaryEggPath, finalRepositoryPath);

         return;


//         foreach (var relativeFilePath in importedRelativeFilePaths) {
//            var importedFile = Path.Combine(modificationRoot, relativeFilePath);
//            var contentFile = Path.Combine(contentDirectory, relativeFilePath);
//            fileSystemProxy.PrepareParentDirectory(contentFile);
//            fileSystemProxy.CopyFile(importedFile, contentFile);
//         }
//
//         var workingDirectoryInfo = fileSystemProxy.GetDirectoryInfo(workingDirectory);
         
//
//         var destinationNestLockPath = Path.Combine(repositoriesDirectory, "LOCK");
//         using (fileSystemProxy.OpenFile(destinationNestLockPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
//            var temporaryNest = Path.Combine(temporaryDirectory, "working_nest");
//            fileSystemProxy.PrepareDirectory(temporaryNest);
//            var inMemoryEgg = new InMemoryEgg(repositoryName, "legacy", workingDirectory);
//            var modificationsNest = new LocalDargonNest(temporaryNest);
//            modificationsNest.InstallEgg(inMemoryEgg);
//
//            var temporaryEggPath = Path.Combine(temporaryNest, inMemoryEgg.Name);
//            fileSystemProxy.MoveDirectory(temporaryEggPath, finalRepositoryPath);
//         }
//
//         fileSystemProxy.DeleteDirectory(temporaryDirectory, true);
      }
   }
}
