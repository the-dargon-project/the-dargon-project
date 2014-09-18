using System.Diagnostics;
using NLog;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public sealed class LeaguePhaseContext : LeaguePhase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly LeaguePhaseData data = new LeaguePhaseData();
      private LeaguePhase currentPhase;

      public LeaguePhaseContext() : base(null) { TransitionTo(new LeaguePhaseUninitialized(this)); }

      public LeaguePhaseData Data { get { return data; } }

      protected override void TransitionTo(LeaguePhase nextPhase, object tag = null)
      {
         logger.Info("Transitioning from " + (currentPhase == null ? "null" : currentPhase.GetType().Name) + " to " + (nextPhase == null ? "null" : nextPhase.GetType().Name));

         var previousPhase = currentPhase;
         if (previousPhase != null)
            previousPhase.EndPhase();
         currentPhase = nextPhase;
         if (nextPhase != null)
            nextPhase.BeginPhase(new BeginPhaseArgs(previousPhase, tag));
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
   }
}