using System;
using System.Collections.Generic;
using Dargon.Processes.Watching;
using ItzWarty.Collections;

namespace Dargon.Game.LeagueOfLegends
{
   public class LeagueProcessWatcher
   {
      /// <summary>
      /// Event fired when the "League of Legends.exe" in-game client launches.
      /// </summary>
      public event LeagueProcessDetectedHandler GameClientLaunched;
      public const string kGameClientProcessNameLower = "league of legends";

      /// <summary>
      /// Event fired when we detect that the League of Legends in-game client crashes.  This is
      /// detected through the launch of BugSplat or the logging of certain data.
      /// </summary>
      public event LeagueProcessDetectedHandler GameClientCrashed;
      public const string kBugSplatProcessNameLower = "bssndrpt"; // ??? unverified

      /// <summary>
      /// Event fired when the League of Legends AIR Client (the "pvp.net client") is launched.
      /// </summary>
      public event LeagueProcessDetectedHandler AirClientLaunched;
      public const string kPvpNetProcessNameLower = "lolclient";
      public const string kPvpNetProcessWindowText = "PVP.net Patcher";

      /// <summary>
      /// Event fired when the League of Legends patcher process is launched.  
      /// </summary>
      public event LeagueProcessDetectedHandler LauncherLaunched;
      public const string kLauncherProcessNameLower = "lollauncher";
      public const string kLauncherProcessNameLower1 = "lollauncher.exe";
      public const string kLauncherProcessNameLower2 = "lol.launcher.exe";

      /// <summary>
      /// Event fired when the RADS_USER_KERNEL.exe process is launched
      /// </summary>
      public event LeagueProcessDetectedHandler RadsUserKernelLaunched;
      public const string kRadsUserKernelProcessNameLower = "rads_user_kernel";

      private readonly ProcessWatcherService processWatcherService;
      private readonly IReadOnlyCollection<string> processNames = ImmutableCollection.Of(kGameClientProcessNameLower, kBugSplatProcessNameLower, kPvpNetProcessNameLower, kLauncherProcessNameLower, kRadsUserKernelProcessNameLower);
      private bool enabled = false;

      /// <summary>
      /// Initializes a new instance of a League of Legends game detector, but does not start the
      /// process detector.
      /// </summary>
      public LeagueProcessWatcher(ProcessWatcherService processWatcherService)
      {
         this.processWatcherService = processWatcherService;
         processWatcherService.Subscribe(HandleNewProcessFound, processNames, false);

         Start();
      }

      /// <summary>
      /// Starts detecting processes and firing events whenever they're launched.
      /// </summary>
      public void Start() { enabled = true; }

      /// <summary>
      /// Stops detecting processes and firing events whenever they're launched.
      /// </summary>
      public void Stop() { enabled = false; }

      /// <summary>
      /// Event handler for when the process watcher detects the launch of a new process.
      /// </summary>
      /// <param name="s"></param>
      /// <param name="e"></param>
      private void HandleNewProcessFound(CreatedProcessDescriptor desc)
      {
         if (!enabled) 
            return;

         string lowerProcessName = desc.ProcessName.ToLower();
         LeagueProcessDetectedHandler @event = null;
         LeagueProcessType processType = LeagueProcessType.Invalid;

         if (lowerProcessName.Contains(kGameClientProcessNameLower)) {
            @event = GameClientLaunched;
            processType = LeagueProcessType.GameClient;
         } else if (lowerProcessName.Contains(kPvpNetProcessNameLower)) {
            @event = AirClientLaunched;
            processType = LeagueProcessType.PvpNetClient;
         } else if (lowerProcessName.Contains(kLauncherProcessNameLower)) {
            @event = LauncherLaunched;
            processType = LeagueProcessType.Launcher;
         } else if (lowerProcessName.Contains(kRadsUserKernelProcessNameLower)) {
            @event = RadsUserKernelLaunched;
            processType = LeagueProcessType.RadsUserKernel;
         } else if (lowerProcessName.Contains(kBugSplatProcessNameLower)) {
            @event = GameClientCrashed;
            processType = LeagueProcessType.BugSplat;
         }

         Console.WriteLine((@event == null) + " " + lowerProcessName + " " + processType);

         if (processType != LeagueProcessType.Invalid) {
            var capture = @event;
            if (capture != null && processType != LeagueProcessType.Invalid)
               capture(new LeagueProcessDetectedArgs(processType, desc));;
         }
      }
   }
}