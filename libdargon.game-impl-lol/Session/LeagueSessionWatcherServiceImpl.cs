using System;
using Dargon.LeagueOfLegends.Processes;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSessionWatcherServiceImpl : LeagueSessionWatcherService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
            var process = GetProcessOrNull(e.ProcessDescriptor.ProcessId);

            if (process == null) {
               logger.Error("League process " + e.ProcessDescriptor.ProcessId + " of type " + e.ProcessType + " quit too quickly!");
               return;
            }

            logger.Info("Handling process " + process.Id + " launch");

            var processTypeList = processesByType[e.ProcessType];
            processTypeList.Add(process);

            process.EnableRaisingEvents = true;
            process.Exited += (a, b) => HandleLeagueProcessQuit(e.ProcessDescriptor.ProcessId, process, e.ProcessType, processTypeList);

            if (process.HasExited) {
               logger.Info("Process " + process.Id + " exited too quickly!");
               processTypeList.Remove(process);
            }

            bool processKilled = false; // todo: event for process detected allowing for duplicate RUK kill
            if (!processKilled) {
               LeagueSession session;
               if (!sessionsByProcessId.TryGetValue(e.ProcessDescriptor.ParentProcessId, out session)) {
                  logger.Info("Creating new session for " + process.Id);
                  session = new LeagueSession();
                  OnSessionCreated(new LeagueSessionCreatedArgs(session));
               }
               logger.Info("Adding process " + process.Id + " to session " + session);
               session.HandleProcessLaunched(process, e.ProcessType);
               sessionsByProcessId.Add(e.ProcessDescriptor.ProcessId, session);
            }
         }
      }

      private Process GetProcessOrNull(int processId)
      {
         try {
            return Process.GetProcessById(processId);
         } catch (ArgumentException e) {
            return null; // process already exited
         }
      }

      private void HandleLeagueProcessQuit(int processId, Process process, LeagueProcessType processType, ISet<Process> processTypeList)
      {
         logger.Info("Handling process " + process.Id + " quit");
         processTypeList.Remove(process);
         LeagueSession session;
         if (sessionsByProcessId.TryGetValue(processId, out session)) {
            logger.Info("Session for " + process.Id + " found!");
            session.HandleProcessQuit(process, processType);
            sessionsByProcessId.Remove(processId);
         } else {
            logger.Error("Session for " + process.Id + " not found!");
         }
      }

      protected virtual void OnSessionCreated(LeagueSessionCreatedArgs e)
      {
         LeagueSessionCreatedHandler handler = SessionCreated;
         if (handler != null) handler(this, e);
      }
   }
}
