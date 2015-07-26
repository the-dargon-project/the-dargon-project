﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Dargon.Client.ViewModels;
using Dargon.Client.ViewModels.Helpers;
using Dargon.Client.Views;
using Dargon.IO.Resolution;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.RADS;
using Dargon.RADS.Manifest;
using ItzWarty;
using ItzWarty.IO;

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
         new Thread(() => {
            foreach (var fileNode in fileNodes) {
               var path = fileNode.Path;
               if (airResolver.Resolve(path).Count > 0) {
                  fileNode.ResolutionState = ResolutionState.ResolutionSuccessful;
               } else if (gameResolver.Resolve(path).Count > 0) {
                  fileNode.ResolutionState = ResolutionState.ResolutionSuccessful;
               } else {
                  fileNode.ResolutionState = ResolutionState.ResolutionFailed;
               }
            }
         }).Start();
         var importWindow = new ModificationImportWindow();
         var modificationImportViewModel = new ModificationImportViewModel(this, importWindow, rootNodeViewModel);
         modificationImportViewModel.ModificationFriendlyName = fileSystemProxy.GetDirectoryInfo(modificationPath).Name;
         importWindow.DataContext = modificationImportViewModel;
         importWindow.ShowDialog();
      }

      public void ImportLegacyModification(string friendlyModificationName, string modificationRoot, string[] importedFilePaths) {
         string repositoryName = Util.ExtractFileNameTokens(friendlyModificationName).Select(token => token.ToLower()).Join("-");
         leagueModificationRepositoryService.ImportLegacyModification(repositoryName, modificationRoot, importedFilePaths, friendlyModificationName);
      }
   }
}