using System;
using System.Diagnostics;
using NLog;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhaseQuit : LeaguePhase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseQuit(LeaguePhaseContext context) : base(context) {
      }

      public override void BeginPhase(BeginPhaseArgs args)
      {
         base.BeginPhase(args);
         logger.Info("Reached Terminal (Quit) League Phase");
      }

      public override void HandleRadsUserKernelLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleLauncherLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandlePatcherLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleLauncherQuit(Process process) { throw new InvalidOperationException(); }
      public override void HandleClientLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleClientQuit(Process process) { throw new InvalidOperationException(); }
      public override void HandleGameLaunched(Process process) { throw new InvalidOperationException(); }
      public override void HandleGameQuit(Process process) { throw new InvalidOperationException(); }
   }
}