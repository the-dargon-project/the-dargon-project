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
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Dargon.Client.Controllers {
   public class ModificationImportController {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly RiotSolutionLoader riotSolutionLoader;
      private readonly ModificationImportViewModelFactory modificationImportViewModelFactory;
      
      public ModificationImportController(IFileSystemProxy fileSystemProxy, LeagueModificationRepositoryService leagueModificationRepositoryService, RiotSolutionLoader riotSolutionLoader, ModificationImportViewModelFactory modificationImportViewModelFactory) {
         this.fileSystemProxy = fileSystemProxy;
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
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

      public void ImportLegacyModification(string friendlyModificationName, string modificationRoot, string[] importedFilePaths) {
         string repositoryName = Util.ExtractFileNameTokens(friendlyModificationName).Select(token => token.ToLower()).Join("-");
         leagueModificationRepositoryService.ImportLegacyModification(repositoryName, modificationRoot, importedFilePaths, friendlyModificationName);
      }
   }
}
