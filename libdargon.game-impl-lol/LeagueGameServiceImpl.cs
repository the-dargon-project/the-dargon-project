using System.IO;
using Dargon.Daemon;
using Dargon.Game;
using Dargon.InjectedModule;
using Dargon.InjectedModule.Tasks;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.FileSystem;
using Dargon.LeagueOfLegends.Lifecycle;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.RADS;
using Dargon.LeagueOfLegends.Session;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.Processes.Watching;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends
{
   public class LeagueGameServiceImpl : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueConfiguration configuration = new LeagueConfiguration();
      private readonly DaemonService daemonService;
      private readonly IProcessProxy processProxy;
      private readonly InjectedModuleService injectedModuleService;
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly ModificationImportService modificationImportService;
      private readonly RadsServiceImpl radsService;
      private readonly ITaskFactory taskFactory;
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService;
      private readonly LeagueModificationTasklistCompilerService leagueModificationTasklistCompilerService;
      private readonly LeagueProcessWatcherServiceImpl leagueProcessWatcherService;
      private readonly LeagueSessionServiceImpl leagueSessionService;
      private readonly RiotFileSystem gameFileSystem;
      private readonly ILeagueInjectedModuleConfigurationFactory leagueInjectedModuleConfigurationFactory;
      private readonly LeagueLifecycleService leagueLifecycleService;

      public LeagueGameServiceImpl(DaemonService daemonService, IProcessProxy processProxy, InjectedModuleService injectedModuleService, ProcessWatcherService processWatcherService, ModificationRepositoryService modificationRepositoryService, ModificationImportService modificationImportService)
      {
         logger.Info("Initializing League Game Service");
         this.daemonService = daemonService;
         this.processProxy = processProxy;
         this.injectedModuleService = injectedModuleService;
         this.processWatcherService = processWatcherService;
         this.modificationRepositoryService = modificationRepositoryService;
         this.modificationImportService = modificationImportService;

         this.radsService = new RadsServiceImpl(configuration.RadsPath);
         this.taskFactory = new TaskFactory();
         this.leagueModificationRepositoryService = new LeagueModificationRepositoryServiceImpl(modificationRepositoryService);
         this.leagueModificationResolutionService = new LeagueModificationResolutionServiceImpl(daemonService, radsService);
         this.leagueModificationObjectCompilerService = new LeagueModificationObjectCompilerServiceImpl(daemonService);
         this.leagueModificationTasklistCompilerService = new LeagueModificationTasklistCompilerServiceImpl(taskFactory);
         this.leagueProcessWatcherService = new LeagueProcessWatcherServiceImpl(processWatcherService);
         this.leagueSessionService = new LeagueSessionServiceImpl(processProxy, leagueProcessWatcherService);
         this.gameFileSystem = new RiotFileSystem(radsService, RiotProjectType.GameClient);
         this.leagueInjectedModuleConfigurationFactory = new LeagueInjectedModuleConfigurationFactory();
         this.leagueLifecycleService = new LeagueLifecycleServiceImpl(injectedModuleService, leagueModificationRepositoryService, leagueModificationResolutionService, leagueModificationObjectCompilerService, leagueModificationTasklistCompilerService, leagueSessionService, radsService, leagueInjectedModuleConfigurationFactory).With(x => x.Initialize());

         RunDebugActions();
      }

      private void RunDebugActions()
      {
         modificationRepositoryService.ClearModifications();
         //         var mod = modificationImportService.ImportLegacyModification(
         //            GameType.LeagueOfLegends,
         //            @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini",
         //            new[] {
         //               @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini\ArtPack\Client\Assets\Images\Champions\Ahri_Square_0.png",
         //               @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini\ArtPack\Client\Assets\Images\Champions\Annie_Square_0.png",
         //               @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini\ArtPack\Client\Characters\Annie\AnnieLoadScreen.dds"
         //            });
         var mod = modificationImportService.ImportLegacyModification(
            GameType.LeagueOfLegends,
            @"C:\lolmodprojects\Tencent Art Pack 8.74",
            Directory.GetFiles(@"C:\lolmodprojects\Tencent Art Pack 8.74\ArtPack\Client\Assets", "*", SearchOption.AllDirectories));
         modificationRepositoryService.AddModification(mod);
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
