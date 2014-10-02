using System.Collections.Concurrent;
using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.Session.Phases;
using ItzWarty;
using NLog;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSession : ILeagueSession
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly object synchronization = new object();
      private readonly LeaguePhaseContext phaseContext;
      private readonly ConcurrentDictionary<LeagueProcessType, IProcess> processesByType = new ConcurrentDictionary<LeagueProcessType, IProcess>(); 
      private IProcess mainPatcherProcess = null;

      public event LeagueSessionProcessLaunchedHandler ProcessLaunched;
      public event LeagueSessionPhaseChangedHandler PhaseChanged;

      public LeagueSession() 
      { 
         logger.Info("At Constructor of new League Session");

         phaseContext = new LeaguePhaseContext(this);
         phaseContext.PhaseChanged += HandlePhaseContextPhaseChanged;
      }

      private void HandlePhaseContextPhaseChanged(ILeagueSession session, LeagueSessionPhaseChangedArgs e) 
      { 
         OnPhaseChanged(this, e); 
      }

      public void HandleProcessLaunched(IProcess process, LeagueProcessType type)
      {
         logger.Info("Dispatching Process Launched " + type);
         OnProcessLaunched(this, new LeagueSessionProcessLaunchedArgs(type, process));

         // only handle the first patcher process
         if (type != LeagueProcessType.Patcher || mainPatcherProcess == null) {
            processesByType.AddOrUpdate(type, process, (a, b) => process);
         } else {
            return;
         }


         if (type == LeagueProcessType.RadsUserKernel)
            phaseContext.HandleRadsUserKernelLaunched(process);
         else if (type == LeagueProcessType.Launcher)
            phaseContext.HandleLauncherLaunched(process);
         else if (type == LeagueProcessType.Patcher) {
            phaseContext.HandlePatcherLaunched(mainPatcherProcess = process);
         } else if (type == LeagueProcessType.PvpNetClient)
            phaseContext.HandleClientLaunched(process);
         else if (type == LeagueProcessType.GameClient)
            phaseContext.HandleGameLaunched(process);
      }

      public void HandleProcessQuit(IProcess process, LeagueProcessType type)
      {
         logger.Info("Dispatching Process Quit " + type);
         if (type != LeagueProcessType.Patcher || process == mainPatcherProcess) {
            IProcess removedProcess;
            processesByType.TryRemove(type, out removedProcess);
         } else {
            return;
         }

         if (type == LeagueProcessType.RadsUserKernel)
            ; // hue
         else if (type == LeagueProcessType.Launcher)
            phaseContext.HandleLauncherQuit(process);
         else if (type == LeagueProcessType.Patcher) {
               phaseContext.HandlePatcherQuit(process);
         } else if (type == LeagueProcessType.PvpNetClient)
            phaseContext.HandleClientQuit(process);
         else if (type == LeagueProcessType.GameClient)
            phaseContext.HandleGameQuit(process);
      }

      public IProcess GetProcessOrNull(LeagueProcessType processType) { return processesByType.GetValueOrDefault(processType); }

      protected virtual void OnProcessLaunched(ILeagueSession session, LeagueSessionProcessLaunchedArgs e)
      {
         LeagueSessionProcessLaunchedHandler handler = ProcessLaunched;
         if (handler != null) handler(session, e);
      }

      protected virtual void OnPhaseChanged(ILeagueSession session, LeagueSessionPhaseChangedArgs e)
      {
         LeagueSessionPhaseChangedHandler handler = PhaseChanged;
         if (handler != null) handler(session, e);
      }
   }
}