using Dargon.Game;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.FileSystem;
using Dargon.LeagueOfLegends.Lifecycle;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.Session;
using Dargon.ModificationRepositories;
using Dargon.Modifications;
using Dargon.Processes.Watching;
using NLog;

namespace Dargon.LeagueOfLegends
{
   public class LeagueGameServiceImpl : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueConfiguration configuration = new LeagueConfiguration();
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly ModificationImportService modificationImportService;
      private readonly LeagueModificationRepositoryServiceImpl leagueModificationRepositoryService;
      private readonly LeagueProcessWatcherServiceImpl leagueProcessWatcherService;
      private readonly LeagueSessionWatcherServiceImpl leagueSessionWatcherService;
      private readonly RiotFileSystem gameFileSystem;
      private readonly LeagueLifecycleService leagueLifecycleService;

      public LeagueGameServiceImpl(ProcessWatcherService processWatcherService, ModificationRepositoryService modificationRepositoryService, ModificationImportService modificationImportService)
      {
         logger.Info("Initializing League Game Service");
         this.processWatcherService = processWatcherService;
         this.modificationRepositoryService = modificationRepositoryService;
         this.modificationImportService = modificationImportService;

         this.leagueModificationRepositoryService = new LeagueModificationRepositoryServiceImpl(modificationRepositoryService);
         this.leagueProcessWatcherService = new LeagueProcessWatcherServiceImpl(processWatcherService);
         this.leagueSessionWatcherService = new LeagueSessionWatcherServiceImpl(leagueProcessWatcherService);
         this.gameFileSystem = new RiotFileSystem(configuration.RadsPath, RiotProjectType.GameClient);
         this.leagueLifecycleService = new LeagueLifecycleServiceImpl(leagueModificationRepositoryService, leagueSessionWatcherService);

         RunDebugActions();
      }

      private void RunDebugActions()
      {
         modificationRepositoryService.ClearModifications();
         var mod = modificationImportService.ImportLegacyModification(
            @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini",
            new[] {
               @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini\ArtPack\Client\Assets\Images\Champions\Ahri_Square_0.png",
               @"C:\lolmodprojects\Tencent Art Pack 8.74 Mini\ArtPack\Client\Assets\Images\Champions\Annie_Square_0.png"
            }, GameType.LeagueOfLegends);
         modificationRepositoryService.AddModification(mod);
      }
   }

   public class LeagueConfiguration
   {
      public string RadsPath { get { return @"V:\Riot Games\League of Legends\RADS"; } }
   }
}
