using System;
using System.Diagnostics;
using NLog;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhaseGame : LeaguePhase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseGame(LeaguePhaseContext context) : base(context) { }

      public override void BeginPhase(BeginPhaseArgs args) 
      {
         base.BeginPhase(args);
         logger.Info("Begin Game League Phase");

         if (args != null) {
            var gameProcess = (Process)args.Tag;
         }
      }

      public override void EndPhase()
      {
         base.EndPhase();
         logger.Info("End Game League Phase");
      }

      public override void HandleRadsUserKernelLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleLauncherLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandlePatcherLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleClientLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleGameLaunched(Process process) { throw new InvalidOperationException(); }

      public override void HandleLauncherQuit(Process process) { base.HandleLauncherQuit(process); }
      public override void HandleClientQuit(Process process) { base.HandleClientQuit(process); }
      public override void HandleGameQuit(Process process) { base.HandleGameQuit(process); TransitionTo(new LeaguePhaseClient(context)); }
   }
}