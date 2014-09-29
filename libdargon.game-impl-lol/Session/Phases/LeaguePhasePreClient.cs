using NLog;
using System;
using System.Diagnostics;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public class LeaguePhasePreClient : LeaguePhaseBase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhasePreClient(LeaguePhaseContext context)
         : base(context, LeagueSessionPhase.Preclient)
      {
      }

      public override void BeginPhase(BeginPhaseArgs args)
      {
         base.BeginPhase(args);
         logger.Info("Begin Pre-Client League Phase");

         HandlePreClientProcessLaunched((Process)args.Tag);
      }

      public override void EndPhase()
      {
         base.EndPhase();
         logger.Info("End Pre-Client League Phase");
      }

      private void HandlePreClientProcessLaunched(Process process)
      {

      }

      public override void HandleRadsUserKernelLaunched(Process process) { HandlePreClientProcessLaunched(process); }
      public override void HandleLauncherLaunched(Process process) { HandlePreClientProcessLaunched(process); }
      public override void HandlePatcherLaunched(Process process) { HandlePreClientProcessLaunched(process); }
      public override void HandleClientLaunched(Process process) { TransitionTo(new LeaguePhaseClient(context)); }
      public override void HandleGameLaunched(Process process) { throw new InvalidOperationException(); }

      public override void HandleLauncherQuit(Process process) { base.HandleLauncherQuit(process); }

      public override void HandlePatcherQuit(Process process)
      {
         base.HandleLauncherQuit(process);
         TransitionTo(new LeaguePhaseQuit(context));
      }

      public override void HandleClientQuit(Process process) { throw new InvalidOperationException(); }
      public override void HandleGameQuit(Process process) { throw new InvalidOperationException(); }
   }
}