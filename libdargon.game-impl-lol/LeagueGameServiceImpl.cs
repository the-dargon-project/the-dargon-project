using System;
using System.Threading;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.FileSystem;
using Dargon.LeagueOfLegends.Lifecycle;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.ModificationRepositories;
using Dargon.Processes;
using Dargon.Processes.Watching;
using ItzWarty;
using ItzWarty.Processes;
using ItzWarty.Threading;
using NLog;
using System.IO;
using System.Linq;
using Dargon.InjectedModule.Commands;

namespace Dargon.LeagueOfLegends
{
   public class LeagueGameServiceImpl : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueConfiguration configuration = new LeagueConfiguration();
      private readonly DaemonService daemonService;
      private readonly TemporaryFileService temporaryFileService;
      private readonly IProcessProxy processProxy;
      private readonly InjectedModuleService injectedModuleService;
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly RadsServiceImpl radsService;
      private readonly ICommandFactory commandFactory;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService;
      private readonly LeagueModificationCommandListCompilerService leagueModificationCommandListCompilerService;
      private readonly LeagueGameModificationLinkerService leagueGameModificationLinkerService;
      private readonly LeagueProcessWatcherServiceImpl leagueProcessWatcherService;
      private readonly LeagueSessionServiceImpl leagueSessionService;
      private readonly RiotFileSystem gameFileSystem;
      private readonly ILeagueInjectedModuleConfigurationFactory leagueInjectedModuleConfigurationFactory;
      private readonly LeagueLifecycleService leagueLifecycleService;

      public LeagueGameServiceImpl(IThreadingProxy threadingProxy, DaemonService daemonService, TemporaryFileService temporaryFileService, IProcessProxy processProxy, InjectedModuleService injectedModuleService, ProcessWatcherService processWatcherService, ModificationRepositoryService modificationRepositoryService)
      {
         logger.Info("Initializing League Game Service");
         this.daemonService = daemonService;
         this.temporaryFileService = temporaryFileService;
         this.processProxy = processProxy;
         this.injectedModuleService = injectedModuleService;
         this.processWatcherService = processWatcherService;
         this.modificationRepositoryService = modificationRepositoryService;

         this.radsService = new RadsServiceImpl(configuration.RadsPath);
         this.commandFactory = new CommandFactory();
         this.leagueModificationRepositoryService = new LeagueModificationRepositoryServiceImpl(modificationRepositoryService);
         this.leagueModificationResolutionService = new LeagueModificationResolutionServiceImpl(threadingProxy, daemonService, radsService);
         this.leagueModificationObjectCompilerService = new LeagueModificationObjectCompilerServiceImpl(threadingProxy, daemonService);
         this.leagueModificationCommandListCompilerService = new LeagueModificationCommandListCompilerServiceImpl(commandFactory);
         this.leagueGameModificationLinkerService = new LeagueGameModificationLinkerServiceImpl(temporaryFileService, radsService, leagueModificationRepositoryService);
         this.leagueProcessWatcherService = new LeagueProcessWatcherServiceImpl(processWatcherService);
         this.leagueSessionService = new LeagueSessionServiceImpl(processProxy, leagueProcessWatcherService);
         this.gameFileSystem = new RiotFileSystem(radsService, RiotProjectType.GameClient);
         this.leagueInjectedModuleConfigurationFactory = new LeagueInjectedModuleConfigurationFactory();
         this.leagueLifecycleService = new LeagueLifecycleServiceImpl(injectedModuleService, leagueModificationRepositoryService, leagueModificationResolutionService, leagueModificationObjectCompilerService, leagueModificationCommandListCompilerService, leagueGameModificationLinkerService, leagueSessionService, radsService, leagueInjectedModuleConfigurationFactory).With(x => x.Initialize());

         RunDebugActions();
      }

      private void RunDebugActions() {
         foreach (var mod in modificationRepositoryService.EnumerateModifications(GameType.Any)) {
            modificationRepositoryService.DeleteModification(mod);
         }

         modificationRepositoryService.ImportLegacyModification(
            "tencent-art-pack",
            @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini",
            Directory.GetFiles(@"C:\lolmodprojects\Tencent Art Pack 8.74 Mini\ArtPack", "*", SearchOption.AllDirectories),
            GameType.LeagueOfLegends);

         // foreach (var mod in modificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends)) {
         //    logger.Info(mod.RepositoryName);
         // 
         //    var metadata = mod.Metadata;
         //    logger.Info("mod: {0} {1} by {2} at {3} for {4}".F(metadata.Name, metadata.Version, metadata.Authors.Join(", "), metadata.Website, metadata.Targets.Select(t=>t.Name).Join(", ")));
         // 
         //    leagueModificationResolutionService.StartModificationResolution(mod, ModificationTargetType.Client | ModificationTargetType.Game).WaitForChainCompletion();
         //    leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client | ModificationTargetType.Game).WaitForChainCompletion();
         //    leagueGameModificationLinkerService.LinkModificationObjects();
         // }
          // for (var mod in modificationRepositoryService)
          // modificationRepositoryService.ClearModifications();
          //         var mod = modificationImportService.ImportLegacyModification(
          //            GameType.LeagueOfLegends,
          //            @"C:\lolmodprojects\Tencent Art Pack 8.74",
          //            Directory.GetFiles(@"C:\lolmodprojects\Tencent Art Pack 8.74\ArtPack\Client\Assets", "*", SearchOption.AllDirectories));
          //         modificationRepositoryService.AddModification(mod);
          //
          // var mod = modificationImportService.ImportLegacyModification(
          //    GameType.LeagueOfLegends,
          //    @"C:\lolmodprojects\Alm1ghty UI 4.4 Foxe Style",
          //    Directory.GetFiles(@"C:\lolmodprojects\Alm1ghty UI 4.4 Foxe Style", "*", SearchOption.AllDirectories));
          // modificationRepositoryService.AddModification(mod);
      }
   }

   public class LeagueConfiguration
   {
      public string RadsPath
      {
         get
         {
            if (Directory.Exists(@"V:\Riot Games\League of Legends\RADS"))
               return @"V:\Riot Games\League of Legends\RADS";
            else
               return @"C:\Riot Games\League of Legends\RADS";
         }
      }
   }
}
