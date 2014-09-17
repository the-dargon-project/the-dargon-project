namespace Dargon.LeagueOfLegends.Processes
{
   public interface LeagueProcessWatcherService
   {
      /// <summary>
      /// Event fired when the "League of Legends.exe" in-game client launches.
      /// </summary>
      event LeagueProcessDetectedHandler GameClientLaunched;

      /// <summary>
      /// Event fired when we detect that the League of Legends in-game client crashes.  This is
      /// detected through the launch of BugSplat or the logging of certain data.
      /// </summary>
      event LeagueProcessDetectedHandler GameClientCrashed;

      /// <summary>
      /// Event fired when the League of Legends AIR Client (the "pvp.net client") is launched.
      /// </summary>
      event LeagueProcessDetectedHandler AirClientLaunched;

      /// <summary>
      /// Event fired when the League of Legends patcher process is launched.  
      /// </summary>
      event LeagueProcessDetectedHandler LauncherLaunched;

      /// <summary>
      /// Event fired when the RADS_USER_KERNEL.exe process is launched
      /// </summary>
      event LeagueProcessDetectedHandler RadsUserKernelLaunched;
   }
}