using System;
using System.Diagnostics;
using NLog;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhaseUninitialized : LeaguePhaseBase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseUninitialized(LeaguePhaseContext context) : base(context, LeagueSessionPhase.Uninitialized) { }

      public override void BeginPhase(BeginPhaseArgs args) { base.BeginPhase(args); logger.Info("Begin Uninitialized League Phase"); }
      public override void EndPhase() { base.EndPhase(); logger.Info("End Uninitialized League Phase"); }

      public override void HandleRadsUserKernelLaunched(Process process) { TransitionTo(new LeaguePhasePreClient(context), process); }
      public override void HandleLauncherLaunched(Process process) { TransitionTo(new LeaguePhasePreClient(context), process); }
      public override void HandlePatcherLaunched(Process process) { TransitionTo(new LeaguePhasePreClient(context), process); }
      public override void HandleClientLaunched(Process process) { throw new NotImplementedException(); }
      public override void HandleGameLaunched(Process process) { throw new NotImplementedException(); }
   }
}