using Dargon.Processes.Watching;
using ItzWarty.Collections;
using NLog;
using System.Collections.Generic;

namespace Dargon.LeagueOfLegends.Processes
{
   public class LeagueProcessWatcherServiceImpl : LeagueProcessWatcherService
   {
      private static Logger logger = LogManager.GetCurrentClassLogger();

      public event LeagueProcessDetectedHandler RadsUserKernelLaunched;
      public const string kRadsUserKernelProcessNameLower = "rads_user_kernel.exe";

      public event LeagueProcessDetectedHandler LauncherLaunched;
      public const string kLauncherProcessNameLower = "lollauncher";
      public const string kLauncherProcessNameLower1 = "lollauncher.exe";

      public event LeagueProcessDetectedHandler PatcherLaunched;
      public const string kPatcherProcessNameLower = "lolpatcher";
      public const string kPatcherProcessNameLower1 = "lolpatcher.exe";

      public event LeagueProcessDetectedHandler AirClientLaunched;
      public const string kPvpNetProcessNameLower = "lolclient";
      public const string kPvpNetProcessNameLower1 = "lolclient.exe";
      public const string kPvpNetProcessWindowText = "PVP.net Patcher";

      public event LeagueProcessDetectedHandler GameClientLaunched;
      public const string kGameClientProcessNameLower = "league of legends.exe";

      public event LeagueProcessDetectedHandler GameClientCrashed;
      public const string kBugSplatProcessNameLower = "bssndrpt"; // ??? unverified

      private readonly ProcessWatcherService processWatcherService;

      private readonly IReadOnlyCollection<string> processNames = ImmutableCollection.Of(
         kRadsUserKernelProcessNameLower,
         kLauncherProcessNameLower, kLauncherProcessNameLower1, //kLauncherProcessNameLower2, kLauncherProcessNameLowerAdmin,
         kPatcherProcessNameLower, kPatcherProcessNameLower1,
         kPvpNetProcessNameLower, kPvpNetProcessNameLower1,
         kGameClientProcessNameLower,
         kBugSplatProcessNameLower
         );

      private bool enabled = false;

      /// <summary>
      /// Initializes a new instance of a League of Legends game detector, but does not start the
      /// process detector.
      /// </summary>
      public LeagueProcessWatcherServiceImpl(ProcessWatcherService processWatcherService)
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

         if (lowerProcessName.Contains(kRadsUserKernelProcessNameLower)) {
            @event = RadsUserKernelLaunched;
            processType = LeagueProcessType.RadsUserKernel;
         } else if (lowerProcessName.Contains(kLauncherProcessNameLower)) {
            @event = LauncherLaunched;
            processType = LeagueProcessType.Launcher;
         } else if (lowerProcessName.Contains(kPatcherProcessNameLower) || lowerProcessName.Contains(kPatcherProcessNameLower1)) {
            @event = PatcherLaunched;
            processType = LeagueProcessType.Patcher;
         } else if (lowerProcessName.Contains(kPvpNetProcessNameLower) || lowerProcessName.Contains(kPvpNetProcessNameLower1)) {
            @event = AirClientLaunched;
            processType = LeagueProcessType.PvpNetClient;
         } else if (lowerProcessName.Contains(kGameClientProcessNameLower)) {
            @event = GameClientLaunched;
            processType = LeagueProcessType.GameClient;
         } else if (lowerProcessName.Contains(kBugSplatProcessNameLower)) {
            @event = GameClientCrashed;
            processType = LeagueProcessType.BugSplat;
         }

         logger.Info((@event == null) + " " + lowerProcessName + " " + processType);

         if (processType != LeagueProcessType.Invalid) {
            var capture = @event;
            if (capture != null && processType != LeagueProcessType.Invalid)
               capture(new LeagueProcessDetectedArgs(processType, desc));
            ;
         }
      }
   }
}