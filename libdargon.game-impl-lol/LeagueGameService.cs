using Dargon.FileSystem;
using Dargon.IO.RADS;
using Dargon.ModificationRepositories;
using Dargon.Processes.Watching;
using NLog;

namespace Dargon.Game.LeagueOfLegends
{
   public class LeagueGameService : IGameHandler
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueConfiguration configuration = new LeagueConfiguration();
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly LeagueProcessWatcher leagueProcessWatcher;
      private readonly RiotFileSystem gameFileSystem;

      public LeagueGameService(ProcessWatcherService processWatcherService, ModificationRepositoryService modificationRepositoryService)
      {
         logger.Info("Initializing League Game Service");
         this.processWatcherService = processWatcherService;
         this.modificationRepositoryService = modificationRepositoryService;

         this.leagueProcessWatcher = new LeagueProcessWatcher(processWatcherService);
         this.gameFileSystem = new RiotFileSystem(configuration.RadsPath, RiotProjectType.GameClient);
      }
   }

   public class LeagueConfiguration
   {
      public string RadsPath { get { return @"V:\Riot Games\League of Legends\RADS"; } }
   }
}
