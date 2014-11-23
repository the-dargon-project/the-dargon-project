using Dargon.Processes;
using ItzWarty.Processes;
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

      public override void HandleRadsUserKernelLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleLauncherLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandlePatcherLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleClientLaunched(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleGameLaunched(IProcess process) { TransitionTo(new LeaguePhaseGame(context), process); }

      public override void HandleLauncherQuit(IProcess process)
      {
         base.HandleLauncherQuit(process);
         TransitionTo(new LeaguePhaseQuit(context));
      }

      public override void HandleClientQuit(IProcess process)
      {
         base.HandleClientQuit(process);
         TransitionTo(new LeaguePhasePreClient(context));
      }

      public override void HandleGameQuit(IProcess process) { throw new InvalidOperationException(); }
   }
}