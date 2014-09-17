using Dargon.Game;
using Dargon.IO.RADS;
using Dargon.LeagueOfLegends.FileSystem;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.Session;
using Dargon.ModificationRepositories;
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
      private readonly LeagueProcessWatcherServiceImpl leagueProcessWatcherService;
      private readonly LeagueSessionWatcherServiceImpl leagueSessionWatcherService;
      private readonly RiotFileSystem gameFileSystem;

      public LeagueGameServiceImpl(ProcessWatcherService processWatcherService, ModificationRepositoryService modificationRepositoryService)
      {
         logger.Info("Initializing League Game Service");
         this.processWatcherService = processWatcherService;
         this.modificationRepositoryService = modificationRepositoryService;

         this.leagueProcessWatcherService = new LeagueProcessWatcherServiceImpl(processWatcherService);
         this.leagueSessionWatcherService = new LeagueSessionWatcherServiceImpl(leagueProcessWatcherService);
         this.gameFileSystem = new RiotFileSystem(configuration.RadsPath, RiotProjectType.GameClient);
      }
   }

   public class LeagueConfiguration
   {
      public string RadsPath { get { return @"V:\Riot Games\League of Legends\RADS"; } }
   }
}
