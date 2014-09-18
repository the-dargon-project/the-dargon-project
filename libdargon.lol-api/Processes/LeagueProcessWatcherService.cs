namespace Dargon.LeagueOfLegends.Processes
{
   public interface LeagueProcessWatcherService
   {
      event LeagueProcessDetectedHandler RadsUserKernelLaunched;
      event LeagueProcessDetectedHandler LauncherLaunched;
      event LeagueProcessDetectedHandler PatcherLaunched;
      event LeagueProcessDetectedHandler AirClientLaunched;
      event LeagueProcessDetectedHandler GameClientLaunched;
      event LeagueProcessDetectedHandler GameClientCrashed;
   }
}