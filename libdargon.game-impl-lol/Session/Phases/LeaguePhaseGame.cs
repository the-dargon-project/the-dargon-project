using NLog;
using System;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhaseGame : LeaguePhaseBase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseGame(LeaguePhaseContext context) : base(context, LeagueSessionPhase.Game) { }

      public override void BeginPhase(BeginPhaseArgs args) 
      {
         base.BeginPhase(args);
         logger.Info("Begin Game League Phase");

         if (args != null) {
            var gameProcess = (IProcess)args.Tag;
         }
      }

      public override void EndPhase()
      {
         base.EndPhase();
         logger.Info("End Game League Phase");
      }

      public override void HandleRadsUserKernelLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleLauncherLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandlePatcherLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleClientLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleGameLaunched(IProcess process) { throw new InvalidOperationException(); }

      public override void HandleLauncherQuit(IProcess process) { base.HandleLauncherQuit(process); }
      public override void HandleClientQuit(IProcess process) { base.HandleClientQuit(process); }
      public override void HandleGameQuit(IProcess process) { base.HandleGameQuit(process); TransitionTo(new LeaguePhaseClient(context)); }
   }
}