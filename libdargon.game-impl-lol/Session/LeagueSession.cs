using Dargon.LeagueOfLegends.Processes;
using Dargon.LeagueOfLegends.Session.Phases;
using NLog;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSession : ILeagueSession
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly object synchronization = new object();
      private readonly LeaguePhaseContext phaseContext;

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

      public void HandleProcessLaunched(Process process, LeagueProcessType type)
      {
         OnProcessLaunched(this, new LeagueSessionProcessLaunchedArgs(type, process));

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