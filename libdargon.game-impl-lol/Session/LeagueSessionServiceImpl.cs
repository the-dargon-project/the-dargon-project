using System;
using Dargon.LeagueOfLegends.Processes;
using System.Collections.Generic;
using System.Diagnostics;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSessionServiceImpl : LeagueSessionService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IProcessProxy processProxy;
      private readonly LeagueProcessWatcherService leagueProcessWatcherService;

      private readonly ISet<LeagueSession> sessions = new HashSet<LeagueSession>();
      private readonly Dictionary<int, LeagueSession> sessionsByProcessId = new Dictionary<int, LeagueSession>();
      private readonly object synchronization = new object();

      public event LeagueSessionCreatedHandler SessionCreated;

      public LeagueSessionServiceImpl(IProcessProxy processProxy,  LeagueProcessWatcherService leagueProcessWatcherService) {
         this.processProxy = processProxy;
         this.leagueProcessWatcherService = leagueProcessWatcherService;

         leagueProcessWatcherService.RadsUserKernelLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.LauncherLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.PatcherLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.AirClientLaunched += HandleLeagueProcessLaunched;
         leagueProcessWatcherService.GameClientLaunched += HandleLeagueProcessLaunched;
      }

      public ILeagueSession GetProcessSessionOrNull(int processId) { return sessionsByProcessId.GetValueOrDefault(processId); }

      private void HandleLeagueProcessLaunched(LeagueProcessDetectedArgs e)
      {
         lock (synchronization) {
            var process = processProxy.GetProcessOrNull(e.ProcessDescriptor.ProcessId);

            if (process == null) {
               logger.Error("League process " + e.ProcessDescriptor.ProcessId + " of type " + e.ProcessType + " quit too quickly!");
               return;
            }

            logger.Info("Handling process " + process.Id + " launch");

            process.EnableRaisingEvents = true;
            process.Exited += (a, b) => HandleLeagueProcessQuit(process, e.ProcessType);

            if (process.HasExited) {
               logger.Info("Process " + process.Id + " exited too quickly!");
            }

            bool processKilled = false; // todo: event for process detected allowing for duplicate RUK kill
            if (!processKilled) {
               LeagueSession session;
               if (!sessionsByProcessId.TryGetValue(e.ProcessDescriptor.ParentProcessId, out session)) {
                  logger.Info("Creating new session for " + process.Id + " as parent process not found " + e.ProcessDescriptor.ParentProcessId);
                  session = new LeagueSession();
                  OnSessionCreated(new LeagueSessionCreatedArgs(session));
               }
               logger.Info("Adding process " + process.Id + " to session " + session);
               session.HandleProcessLaunched(process, e.ProcessType);
               sessionsByProcessId.Add(e.ProcessDescriptor.ProcessId, session);
            }
         }
      }

      private void HandleLeagueProcessQuit(IProcess process, LeagueProcessType processType)
      {
         logger.Info("Handling process " + process.Id + " quit");
         LeagueSession session;
         if (sessionsByProcessId.TryGetValue(process.Id, out session)) {
            logger.Info("Session for " + process.Id + " found!");
            session.HandleProcessQuit(process, processType);
            sessionsByProcessId.Remove(process.Id);
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
