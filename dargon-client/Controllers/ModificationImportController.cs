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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Dargon.Modifications;
using Dargon.Nest;
using Dargon.Patcher;
using Dargon.PortableObjects;

namespace Dargon.Client.Controllers {
   public class ModificationImportController {
      private readonly string repositoriesDirectory;
      private readonly TemporaryFileService temporaryFileService;
      private readonly ModificationComponentFactory modificationComponentFactory;
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly RiotSolutionLoader riotSolutionLoader;
      private readonly ModificationImportViewModelFactory modificationImportViewModelFactory;
      
      public ModificationImportController(string repositoriesDirectory, TemporaryFileService temporaryFileService, ModificationComponentFactory modificationComponentFactory, IFileSystemProxy fileSystemProxy, RiotSolutionLoader riotSolutionLoader, ModificationImportViewModelFactory modificationImportViewModelFactory) {
         this.repositoriesDirectory = repositoriesDirectory;
         this.temporaryFileService = temporaryFileService;
         this.modificationComponentFactory = modificationComponentFactory;
         this.fileSystemProxy = fileSystemProxy;
         this.riotSolutionLoader = riotSolutionLoader;
         this.modificationImportViewModelFactory = modificationImportViewModelFactory;
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
                     if (file.ResolutionPath.IndexOf("DATA/Characters", StringComparison.OrdinalIgnoreCase) != -1) {
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
               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                  modificationImportViewModel.ModificationCategorization = modificationType;
               }));
            }
         }).Start();
         importWindow.ShowDialog();
      }

      public void ImportLegacyModification(string friendlyModificationName, string modificationRoot, string[] importedFilePaths, LeagueModificationCategory category) {
         modificationRoot = Path.GetFullPath(modificationRoot);
         var importedRelativeFilePaths = Util.Generate(importedFilePaths.Length, i => Path.GetFullPath(importedFilePaths[i]).Substring(modificationRoot.Length + 1));

         string repositoryName = Util.ExtractFileNameTokens(friendlyModificationName).Select(token => token.ToLower()).Join("-");
         string finalRepositoryPath = Path.Combine(repositoriesDirectory, repositoryName);
         var temporaryDirectory = temporaryFileService.AllocateTemporaryDirectory(TimeSpan.FromHours(1));
         var workingDirectory = Path.Combine(temporaryDirectory, "working");
         var contentDirectory = Path.Combine(workingDirectory, "content");
         fileSystemProxy.PrepareDirectory(contentDirectory);

         foreach (var relativeFilePath in importedRelativeFilePaths) {
            var importedFile = Path.Combine(modificationRoot, relativeFilePath);
            var contentFile = Path.Combine(contentDirectory, relativeFilePath);
            fileSystemProxy.PrepareParentDirectory(contentFile);
            fileSystemProxy.CopyFile(importedFile, contentFile);
         }

         var workingDirectoryInfo = fileSystemProxy.GetDirectoryInfo(workingDirectory);
         var modification = new ModificationImpl(workingDirectoryInfo.Name, workingDirectoryInfo.FullName, modificationComponentFactory);
         var info = modification.GetComponent<InfoComponent>();
         info.Id = Guid.NewGuid();
         info.Name = friendlyModificationName;

         var leagueComponent = modification.GetComponent<LeagueMetadataComponent>();
         leagueComponent.Category = category;

         var destinationNestLockPath = Path.Combine(repositoriesDirectory, "LOCK");
         using (fileSystemProxy.OpenFile(destinationNestLockPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
            var temporaryNest = Path.Combine(temporaryDirectory, "working_nest");
            fileSystemProxy.PrepareDirectory(temporaryNest);
            var inMemoryEgg = new InMemoryEgg(repositoryName, "legacy", workingDirectory);
            var modificationsNest = new LocalDargonNest(temporaryNest);
            modificationsNest.InstallEgg(inMemoryEgg);

            var temporaryEggPath = Path.Combine(temporaryNest, inMemoryEgg.Name);
            fileSystemProxy.MoveDirectory(temporaryEggPath, finalRepositoryPath);
         }

         fileSystemProxy.DeleteDirectory(temporaryDirectory, true);
      }
   }
}
