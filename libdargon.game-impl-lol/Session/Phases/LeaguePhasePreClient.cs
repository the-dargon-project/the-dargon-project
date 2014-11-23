using Dargon.Processes;
using ItzWarty.Processes;
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

         HandlePreClientProcessLaunched((IProcess)args.Tag);
      }

      public override void EndPhase()
      {
         base.EndPhase();
         logger.Info("End Pre-Client League Phase");
      }

      private void HandlePreClientProcessLaunched(IProcess process)
      {

      }

      public override void HandleRadsUserKernelLaunched(IProcess process) { HandlePreClientProcessLaunched(process); }
      public override void HandleLauncherLaunched(IProcess process) { HandlePreClientProcessLaunched(process); }
      public override void HandlePatcherLaunched(IProcess process) { HandlePreClientProcessLaunched(process); }
      public override void HandleClientLaunched(IProcess process) { TransitionTo(new LeaguePhaseClient(context)); }
      public override void HandleGameLaunched(IProcess process) { throw new InvalidOperationException(); }

      public override void HandleLauncherQuit(IProcess process) { base.HandleLauncherQuit(process); }

      public override void HandlePatcherQuit(IProcess process)
      {
         base.HandleLauncherQuit(process);
         TransitionTo(new LeaguePhaseQuit(context));
      }

      public override void HandleClientQuit(IProcess process) { throw new InvalidOperationException(); }
      public override void HandleGameQuit(IProcess process) { throw new InvalidOperationException(); }
   }
}