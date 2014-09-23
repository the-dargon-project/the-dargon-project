using NLog;
using System;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhaseClient : LeaguePhaseBase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseClient(LeaguePhaseContext context) : base(context, LeagueSessionPhase.Client) { }

      public override void BeginPhase(BeginPhaseArgs args) 
      {
         base.BeginPhase(args);
         logger.Info("Begin Client League Phase");

         if (args != null) {
            var clientProcess = (Process)args.Tag;
         }
      }

      public override void EndPhase()
      {
         base.EndPhase();
         logger.Info("End Client League Phase");
      }

      public override void HandleRadsUserKernelLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleLauncherLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandlePatcherLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleClientLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleGameLaunched(Process process) { TransitionTo(new LeaguePhaseGame(context), process); }

      public override void HandleLauncherQuit(Process process)
      {
         base.HandleLauncherQuit(process);
         TransitionTo(new LeaguePhaseQuit(context));
      }

      public override void HandleClientQuit(Process process)
      {
         base.HandleClientQuit(process);
         TransitionTo(new LeaguePhasePreClient(context));
      }

      public override void HandleGameQuit(Process process) { throw new InvalidOperationException(); }
   }
}