using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public abstract class LeaguePhaseBase
   {
      protected readonly LeaguePhaseContext context;
      private readonly LeagueSessionPhase phase;

      protected LeaguePhaseBase(LeaguePhaseContext context, LeagueSessionPhase phase)
      {
         this.context = context;
         this.phase = phase;
      }

      public virtual LeagueSessionPhase Phase { get { return phase; } }

      protected virtual void TransitionTo(LeaguePhaseBase nextPhase, object tag = null) { context.TransitionTo(nextPhase, tag); }

      public virtual void BeginPhase(BeginPhaseArgs args) { }
      public virtual void EndPhase() { }

      public abstract void HandleRadsUserKernelLaunched(Process process);
      public abstract void HandleLauncherLaunched(Process process);
      public abstract void HandlePatcherLaunched(Process process);
      public abstract void HandleClientLaunched(Process process);
      public abstract void HandleGameLaunched(Process process);

      public virtual void HandlePatcherQuit(Process process) { }
      public virtual void HandleLauncherQuit(Process process) { }
      public virtual void HandleClientQuit(Process process) { }
      public virtual void HandleGameQuit(Process process) { }

      public class BeginPhaseArgs
      {
         private readonly LeaguePhaseBase previousPhase;
         private readonly object tag;

         public BeginPhaseArgs(LeaguePhaseBase previousPhase, object tag)
         {
            this.previousPhase = previousPhase;
            this.tag = tag;
         }

         public LeaguePhaseBase PreviousPhase { get { return previousPhase; } }
         public object Tag { get { return tag; } }
      }
   }
}
