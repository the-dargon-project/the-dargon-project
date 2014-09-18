using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public abstract class LeaguePhase
   {
      protected readonly LeaguePhaseContext context;

      protected LeaguePhase(LeaguePhaseContext context) {
         this.context = context;
      }

      protected virtual void TransitionTo(LeaguePhase nextPhase, object tag = null) { context.TransitionTo(nextPhase, tag); }

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
         private readonly LeaguePhase previousPhase;
         private readonly object tag;

         public BeginPhaseArgs(LeaguePhase previousPhase, object tag)
         {
            this.previousPhase = previousPhase;
            this.tag = tag;
         }

         public LeaguePhase PreviousPhase { get { return previousPhase; } }
         public object Tag { get { return tag; } }
      }
   }
}
