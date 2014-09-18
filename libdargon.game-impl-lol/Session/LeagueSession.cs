using System;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.Session.Phases;
using NLog;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSession
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly object synchronization = new object();
      private readonly LeaguePhaseContext phaseContext = new LeaguePhaseContext();

      public LeagueSession() 
      { 
         logger.Info("At Constructor of new League Session");
      }

      public void HandleProcessLaunched(Process process, LeagueProcessType type)
      {
         if (type == LeagueProcessType.RadsUserKernel)
            phaseContext.HandleRadsUserKernelLaunched(process);
         else if (type == LeagueProcessType.Launcher)
            phaseContext.HandleLauncherLaunched(process);
         else if (type == LeagueProcessType.Patcher)
            phaseContext.HandlePatcherLaunched(process);
         else if (type == LeagueProcessType.PvpNetClient)
            phaseContext.HandleClientLaunched(process);
         else if (type == LeagueProcessType.GameClient)
            phaseContext.HandleGameLaunched(process);
      }

      public void HandleProcessQuit(Process process, LeagueProcessType type)
      {
         if (type == LeagueProcessType.RadsUserKernel)
            ; // hue
         else if (type == LeagueProcessType.Launcher)
            phaseContext.HandleLauncherQuit(process);
         else if(type == LeagueProcessType.Patcher)
            phaseContext.HandlePatcherQuit(process);
         else if(type == LeagueProcessType.PvpNetClient)
            phaseContext.HandleClientQuit(process);
         else if (type == LeagueProcessType.GameClient)
            phaseContext.HandleGameQuit(process);
      }
   }
}

//      private void HandleRadsUserKernelLaunched(Process process)
//      {
//         lock (synchronization) {
//            sessionStateType = StateType.Launching;
//            if (!m_lol.m_resourcesSuspended) {
//               var progressWindow = m_progressPlugin.CreateNew();
//               progressWindow.Update(
//                  "Dargon RUK",
//                  "Preparing for patcher launch",
//                  "Just in case the game patches",
//                  0
//                  );
//               m_lol.SuspendResourceSubsystem();
//               progressWindow.Close();
//            }
//         }
//      }
//
//      private void HandleLauncherLaunched(Process process)
//      {
//         lock (synchronization) {
//            m_launcherProcess = e.Process;
//            sessionStateType = StateType.Launcher;
//            if (!m_lol.m_resourcesSuspended) {
//               var progressWindow = m_progressPlugin.CreateNew();
//               progressWindow.Update(
//                  "Dargon LAUN",
//                  "Preparing for patcher launch",
//                  "Just in case the game patches",
//                  0
//                  );
//               m_lol.SuspendResourceSubsystem();
//               progressWindow.Close();
//            }
//
//            InjectLauncherDll(e.ProcessID);
//         }
//      }
//
//      private void HandlePvpNetLaunched(Process process)
//      {
//         m_airProcess = e.Process;
//         sessionStateType = StateType.PvpNetClient;
//
//         var progressWindow = m_progressPlugin.CreateNew();
//         progressWindow.Update(
//            "Dargon",
//            "Preparing Dargon Modifications",
//            "This shouldn't take too long",
//            0
//            );
//
//         // When the Pvp.Net client launches, 
//         Parallel.Invoke(
//            () => {
//               m_lol.ResumeResourceSubsystem();
//               m_changeAggregator.BeginAggregate(m_lol.Modifications);
//            },
//            () => InjectAirDll(e.ProcessID)
//            );
//         progressWindow.Close();
//      }
//
//      private void HandleGameLaunched(Process process)
//      {
//         //         // we currently don't support lolreplay-spawned games.
//         //         if (m_rootProcessType != LolProcessType.GameClient) {
//         //            m_gameProcess = e.Process;
//         //            sessionState = LeagueSessionState.OfficialGame;
//         //            InjectGameDll(e.ProcessID);
//         //         }
//      }
//
//      /// <summary>
//      /// Injects Dargon-Maestro into Maestro, the application which handles launching the League
//      /// of Legends AIR Client and Game Clients.  Dargon-Maestro suspends the launched applications
//      /// so that Dargon-LoL can apply its in-memory changes without any race conditions.
//      /// </summary>
//      /// <param name="e"></param>
//      private void InjectLauncherDll(int processId)
//      {
//         m_lol.Dargon.InjectedModuleManager.InjectToProcess(
//            processId,
//            m_lol.PluginReleaseManifest.Files[LolGuids.kDargonInjectedDllGuid].Path,
//            new List<string>() { "--debug" },
//            new List<KeyValuePair<string, string>> {
//               new KeyValuePair<string, string>("role", "launcher"),
//               new KeyValuePair<string, string>("log_filter", "verbose"),
//               new KeyValuePair<string, string>("launchsuspended", "LolClient.exe"),
//               new KeyValuePair<string, string>("launchsuspended", "League of Legends.exe")
//            },
//            (ctx) => ctx.DspExSession.AddInstructionSet(new LolDspExInstructionSet(this, ctx, LolProcessType.Launcher)),
//            (ctx) => { }
//            );
//      }
//
//      /// <summary>
//      /// Injects the Dargon-LoL dll to the given process.  Dargon-LoL redirects various API calls
//      /// so that our modifications and other run-time actions can occur.  After initializing,
//      /// Dargon-LoL resumes the main League of Legends thr ead.
//      /// </summary>
//      /// <param name="e"></param>
//      private void InjectAirDll(int processId)
//      {
//         m_lol.Dargon.InjectedModuleManager.InjectToProcess(
//            processId,
//            m_lol.PluginReleaseManifest.Files[LolGuids.kDargonInjectedDllGuid].Path,
//            new List<string>() { "--debug", "--enable-dim-tasklist", "--enable-filesystem-hooks", "--enable-filesystem-mods" },
//            new List<KeyValuePair<string, string>> {
//               new KeyValuePair<string, string>("role", "air"),
//               new KeyValuePair<string, string>("log_filter", "verbose")
//            },
//            (ctx) => ctx.DspExSession.AddInstructionSet(new LolDspExInstructionSet(this, ctx, LolProcessType.PvpNetClient)),
//            (ctx) => { }
//            );
//      }
//
//      /// <summary>
//      /// Injects the Dargon-LoL dll to the given process.  Dargon-LoL redirects various API calls
//      /// so that our modifications and other run-time actions can occur.  After initializing,
//      /// Dargon-LoL resumes the main League of Legends thread.
//      /// </summary>
//      /// <param name="e"></param>
//      private void InjectGameDll(int processId)
//      {
//         m_lol.Dargon.InjectedModuleManager.InjectToProcess(
//            processId,
//            m_lol.PluginReleaseManifest.Files[LolGuids.kDargonInjectedDllGuid].Path,
//            new List<string>() { "--debug", "--enable-dim-tasklist", "--enable-filesystem-hooks", "--enable-filesystem-mods", "--enable-raf-mods" },
//            new List<KeyValuePair<string, string>> {
//               new KeyValuePair<string, string>("role", "game"),
//               new KeyValuePair<string, string>("log_filter", "verbose")
//            },
//            (ctx) => ctx.DspExSession.AddInstructionSet(new LolDspExInstructionSet(this, ctx, LolProcessType.GameClient)),
//            (ctx) => { }
//            );
//      }

      //
      //      private void BuildTaskList() { DIMTaskList.Build(m_lol.Dargon, m_changeAggregator.GetResult().NonconflictingChanges, m_lol.Dargon.AddInManager.Enumerate<IChangeApplicator>()); }
      //
      //      public IEnumerable<IChange> GetAirChanges()
      //      {
      //         return m_changeAggregator.GetResult().NonconflictingChanges
      //            .Where((change) => ((LolChangeUserFlags)change.UserFlags).HasFlag(LolChangeUserFlags.SEND_TO_AIR));
      //      }
      //
      //      public void SendInitialTaskList(LolDspExInstructionSet context)
      //      {
      //         Logger.L(LoggerLevel.Always, "Sending Initial Task List for " + context.LolProcessType);
      //         if (context.LolProcessType == LolProcessType.PvpNetClient) {
      //            var list = DIMTaskList.Build(m_lol.Dargon, GetAirChanges(), m_lol.Dargon.AddInManager.Enumerate<IChangeApplicator>());
      //            SendTaskList(context, list, true);
      //         }
      //      }
      //
      //      private void SendTaskList(LolDspExInstructionSet context, DIMTaskList tasks, bool forceSend = false)
      //      {
      //         Logger.L(LoggerLevel.Always, "Sending Task List for " + context.LolProcessType);
      //         var session = context.BootstrapContext.DspExSession;
      //         var handler = new DSPExLITProcessTaskList(session.TakeLocallyInitializedTransactionId(), tasks, forceSend);
      //         session.RegisterAndInitializeLITransactionHandler(handler);
      //      }