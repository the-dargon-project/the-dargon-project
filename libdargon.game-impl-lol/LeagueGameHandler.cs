using Dargon.FileSystem;
using Dargon.IO.RADS;
using Dargon.ModificationRepositories;
using Dargon.Processes.Watching;

namespace Dargon.Game.LeagueOfLegends
{
   public class LeagueGameHandler : IGameHandler
   {
      private readonly LeagueConfiguration configuration = new LeagueConfiguration();
      private readonly ProcessWatcherService processWatcherService;
      private readonly ModificationRepositoryService modificationRepositoryService;
      private readonly LeagueProcessWatcher leagueProcessWatcher;
      private readonly RiotFileSystem gameFileSystem;

      public LeagueGameHandler(ProcessWatcherService processWatcherService, ModificationRepositoryService modificationRepositoryService)
      {
         this.processWatcherService = processWatcherService;
         this.modificationRepositoryService = modificationRepositoryService;

         this.leagueProcessWatcher = new LeagueProcessWatcher(processWatcherService);
         this.gameFileSystem = new RiotFileSystem(configuration.RadsPath, RiotProjectType.GameClient);
      }
   }

   public class LeagueConfiguration
   {
      public string RadsPath { get { return @"C:\Riot Games\League of Legends\RADS"; } }
   }
}
