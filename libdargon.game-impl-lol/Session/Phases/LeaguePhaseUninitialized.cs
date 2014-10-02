using NLog;
using System;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhaseUninitialized : LeaguePhaseBase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseUninitialized(LeaguePhaseContext context) : base(context, LeagueSessionPhase.Uninitialized) { }

      public override void BeginPhase(BeginPhaseArgs args) { base.BeginPhase(args); logger.Info("Begin Uninitialized League Phase"); }
      public override void EndPhase() { base.EndPhase(); logger.Info("End Uninitialized League Phase"); }

      public override void HandleRadsUserKernelLaunched(IProcess process) { TransitionTo(new LeaguePhasePreClient(context), process); }
      public override void HandleLauncherLaunched(IProcess process) { TransitionTo(new LeaguePhasePreClient(context), process); }
      public override void HandlePatcherLaunched(IProcess process) { TransitionTo(new LeaguePhasePreClient(context), process); }
      public override void HandleClientLaunched(IProcess process) { throw new NotImplementedException(); }
      public override void HandleGameLaunched(IProcess process) { throw new NotImplementedException(); }
   }
}