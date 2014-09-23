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
   public class LeagueSessionWatcherServiceImpl : LeagueSessionWatcherService
   {
      private readonly LeagueProcessWatcherService leagueProcessWatcherService;

      private readonly ISet<Process> radsUserKernelProcesses = new HashSet<Process>();
      private readonly ISet<Process> launcherProcesses = new HashSet<Process>();
      private readonly ISet<Process> patcherProcesses = new HashSet<Process>();
      private readonly ISet<Process> pvpNetClientProcesses = new HashSet<Process>();
      private readonly ISet<Process> gameClientProcesses = new HashSet<Process>();
      private readonly ISet<Process> bugsplatProcesses = new HashSet<Process>();
      private readonly ISet<LeagueSession> sessions = new HashSet<LeagueSession>();
      private readonly Dictionary<int, LeagueSession> sessionsByProcessId = new Dictionary<int, LeagueSession>();
      private readonly object synchronization = new object();
      private readonly IReadOnlyDictionary<LeagueProcessType, ISet<Process>> processesByType;
      public event LeagueSessionCreatedHandler SessionCreated;

      public LeagueSessionWatcherServiceImpl(LeagueProcessWatcherService leagueProcessWatcherService) {
         this.leagueProcessWatcherService = leagueProcessWatcherService;

         processesByType = new Dictionary<LeagueProcessType, ISet<Process>> {
            { LeagueProcessType.RadsUserKernel, radsUserKernelProcesses },
            { LeagueProcessType.Launcher, launcherProcesses },
            { LeagueProcessType.Patcher, patcherProcesses },
            { LeagueProcessType.PvpNetClient, pvpNetClientProcesses },
            { LeagueProcessType.GameClient, gameClientProcesses },
            { LeagueProcessType.BugSplat, bugsplatProcesses }
         };

         leagueProcessWatcherService.RadsUserKernelLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.LauncherLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.PatcherLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.AirClientLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.GameClientLaunched += HandleLeagueProcessLaunched;
      }

      private void HandleLeagueProcessLaunched(LeagueProcessDetectedArgs e)
      {
         lock (synchronization) {
            var process = Process.GetProcessById(e.ProcessDescriptor.ProcessId);

            var processTypeList = processesByType[e.ProcessType];
            processTypeList.Add(process);

            process.EnableRaisingEvents = true;
            process.Exited += (a, b) => HandleLeagueProcessQuit(e.ProcessDescriptor.ProcessId, process, e.ProcessType, processTypeList);

            if (process.HasExited) {
               processTypeList.Remove(process);
            }

            bool processKilled = false; // todo: event for process detected allowing for duplicate RUK kill
            if (!processKilled) {
               LeagueSession session;
               if (!sessionsByProcessId.TryGetValue(e.ProcessDescriptor.ParentProcessId, out session)) {
                  session = new LeagueSession();
                  OnSessionCreated(new LeagueSessionCreatedArgs(session));
               }
               session.HandleProcessLaunched(process, e.ProcessType);
               sessionsByProcessId.Add(e.ProcessDescriptor.ProcessId, session);
            }
         }
      }

      private void HandleLeagueProcessQuit(int processId, Process process, LeagueProcessType processType, ISet<Process> processTypeList) 
      { 
         processTypeList.Remove(process);
         LeagueSession session;
         if (sessionsByProcessId.TryGetValue(processId, out session)) {
            session.HandleProcessQuit(process, processType);
            sessionsByProcessId.Remove(processId);
         }
      }

      protected virtual void OnSessionCreated(LeagueSessionCreatedArgs e)
      {
         LeagueSessionCreatedHandler handler = SessionCreated;
         if (handler != null) handler(this, e);
      }
   }
}
