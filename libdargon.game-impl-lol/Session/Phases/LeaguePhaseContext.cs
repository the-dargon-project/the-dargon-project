using System;
using System.Diagnostics;
using ItzWarty;
using NLog;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public sealed class LeaguePhaseContext : LeaguePhaseBase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeagueSession session;
      private readonly LeaguePhaseData data = new LeaguePhaseData();
      public event LeagueSessionPhaseChangedHandler PhaseChanged;
      private LeaguePhaseBase currentPhase;

      public LeaguePhaseContext(LeagueSession session) : base(null, LeagueSessionPhase.Uninitialized)
      {
         this.session = session;
         TransitionTo(new LeaguePhaseUninitialized(this));
      }

      public LeaguePhaseData Data { get { return data; } }
      public override LeagueSessionPhase Phase { get { return currentPhase.Phase; } }

      protected override void TransitionTo(LeaguePhaseBase nextPhase, object tag = null)
      {
         nextPhase.ThrowIfNull("nextPhase");

         logger.Info("Transitioning from " + (currentPhase == null ? "null" : currentPhase.GetType().Name) + " to " + (nextPhase == null ? "null" : nextPhase.GetType().Name));

         var previousPhase = currentPhase;
         if (previousPhase != null)
            previousPhase.EndPhase();
         currentPhase = nextPhase;
         if (nextPhase != null)
            nextPhase.BeginPhase(new BeginPhaseArgs(previousPhase, tag));

         OnPhaseChanged(new LeagueSessionPhaseChangedArgs(previousPhase == null ? LeagueSessionPhase.Uninitialized : previousPhase.Phase, currentPhase.Phase));
      }

      public override void HandleRadsUserKernelLaunched(Process process) { currentPhase.HandleRadsUserKernelLaunched(process); }
      public override void HandleLauncherLaunched(Process process) { currentPhase.HandleLauncherLaunched(process); }
      public override void HandleClientLaunched(Process process) { currentPhase.HandleClientLaunched(process); }
      public override void HandleGameLaunched(Process process) { currentPhase.HandleGameLaunched(process); }
      public override void HandlePatcherLaunched(Process process) { currentPhase.HandlePatcherLaunched(process); }

      public override void HandlePatcherQuit(Process process) { currentPhase.HandlePatcherQuit(process); }
      public override void HandleLauncherQuit(Process process) { currentPhase.HandleLauncherQuit(process); }
      public override void HandleClientQuit(Process process) { currentPhase.HandleClientQuit(process); }
      public override void HandleGameQuit(Process process) { currentPhase.HandleGameQuit(process); }

      private void OnPhaseChanged(LeagueSessionPhaseChangedArgs e)
      {
         LeagueSessionPhaseChangedHandler handler = PhaseChanged;
         if (handler != null) 
            handler(session, e);
      }
   }
}