using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dargon.LeagueOfLegends.Processes;
using ItzWarty;
using ItzWarty.Collections;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSessionWatcherServiceImpl
   {
      private readonly LeagueProcessWatcherService leagueProcessWatcherService;

      private readonly ConcurrentSet<Process> radsUserKernelProcesses = new ConcurrentSet<Process>();
      private readonly ConcurrentSet<Process> launcherProcesses = new ConcurrentSet<Process>();
      private readonly ConcurrentSet<Process> pvpNetClientProcesses = new ConcurrentSet<Process>();
      private readonly ConcurrentSet<Process> gameClientProcesses = new ConcurrentSet<Process>();
      private readonly ConcurrentSet<Process> bugsplatProcesses = new ConcurrentSet<Process>();
      private readonly ConcurrentSet<LeagueSession> sessions = new ConcurrentSet<LeagueSession>();
      private readonly ConcurrentDictionary<int, LeagueSession> sessionsByProcessId = new ConcurrentDictionary<int, LeagueSession>(); 
      private readonly IReadOnlyDictionary<LeagueProcessType, ConcurrentSet<Process>> processesByType;

      public LeagueSessionWatcherServiceImpl(LeagueProcessWatcherService leagueProcessWatcherService) {
         this.leagueProcessWatcherService = leagueProcessWatcherService;

         processesByType = new Dictionary<LeagueProcessType, ConcurrentSet<Process>> {
            { LeagueProcessType.RadsUserKernel, radsUserKernelProcesses },
            { LeagueProcessType.Launcher, launcherProcesses },
            { LeagueProcessType.PvpNetClient, pvpNetClientProcesses },
            { LeagueProcessType.GameClient, gameClientProcesses },
            { LeagueProcessType.BugSplat, bugsplatProcesses }
         };

         leagueProcessWatcherService.AirClientLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.GameClientLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.LauncherLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.RadsUserKernelLaunched += HandleLeagueProcessLaunched;
      }

      private void HandleLeagueProcessLaunched(LeagueProcessDetectedArgs e)
      {
         var process = Process.GetProcessById(e.ProcessDescriptor.ProcessId);

         var processTypeList = processesByType[e.ProcessType];
         processTypeList.TryAdd(process);

         process.EnableRaisingEvents = true;
         process.Exited += (a, b) => processTypeList.TryRemove(process);

         if (process.HasExited) {
            processTypeList.TryRemove(process);
         }

         bool processKilled = false; // todo: event for process detected allowing for duplicate RUK kill
         if (!processKilled) {
            LeagueSession session;
            if (!sessionsByProcessId.TryGetValue(e.ProcessDescriptor.ParentProcessId, out session)) {
               session = new LeagueSession();
            }
            session.HandleProcessLaunched(process, e.ProcessType);
            sessionsByProcessId.AddOrUpdate(e.ProcessDescriptor.ProcessId, session, (a, b) => session);
         }
      }
   }
}
