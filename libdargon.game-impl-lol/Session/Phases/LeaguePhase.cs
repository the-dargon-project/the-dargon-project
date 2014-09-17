using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Dargon.LeagueOfLegends.Session.Phases
{
   public sealed class LeaguePhaseContext : LeaguePhase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private LeaguePhase currentPhase;
      private LeaguePhaseData data = new LeaguePhaseData();

      public LeaguePhaseContext() : base(null) { TransitionTo(new LeaguePhaseUninitialized(this)); }

      public LeaguePhaseData Data { get { return data; } }

      protected override void TransitionTo(LeaguePhase nextPhase, object tag = null)
      {
         logger.Info("Transitioning from " + (currentPhase == null ? "null" : currentPhase.GetType().Name) + " to " + (nextPhase == null ? "null" : nextPhase.GetType().Name));

         var previousPhase = currentPhase;
         if (previousPhase != null)
            previousPhase.EndPhase();
         currentPhase = nextPhase;
         if (nextPhase != null)
            nextPhase.BeginPhase(new BeginPhaseArgs(previousPhase, tag));
      }

      public override void HandleRadsUserKernelLaunched(Process process) { currentPhase.HandleRadsUserKernelLaunched(process); }

      public override void HandleLauncherLaunched(Process process) { currentPhase.HandleLauncherLaunched(process); }

      public override void HandlePvpNetLaunched(Process process) { currentPhase.HandlePvpNetLaunched(process); }

      public override void HandleGameLaunched(Process process) { currentPhase.HandleGameLaunched(process); }
   }

   public class LeaguePhaseData
   {

   }

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
      public abstract void HandlePvpNetLaunched(Process process);
      public abstract void HandleGameLaunched(Process process);

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

   public class LeaguePhaseUninitialized : LeaguePhase
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public LeaguePhaseUninitialized(LeaguePhaseContext context) : base(context) { }

      public override void BeginPhase(BeginPhaseArgs args) { base.BeginPhase(args); logger.Info("Begin Uninitialized League Phase"); }

      public override void EndPhase() { base.EndPhase(); logger.Info("End Uninitialized League Phase"); }

      public override void HandleRadsUserKernelLaunched(Process process) { TransitionTo(new LeaguePhasePrePvpNet(context), process); }

      public override void HandleLauncherLaunched(Process process) { TransitionTo(new LeaguePhasePrePvpNet(context), process); }

      public override void HandlePvpNetLaunched(Process process) { throw new NotImplementedException(); }

      public override void HandleGameLaunched(Process process) { throw new NotImplementedException(); }
   }

   public class LeaguePhasePrePvpNet : LeaguePhase
   {
      public LeaguePhasePrePvpNet(LeaguePhaseContext context) : base(context) {
      }

      public override void BeginPhase(BeginPhaseArgs args)
      {
         base.BeginPhase(args);

         HandlePrePvpNetProcessLaunched((Process)args.Tag);
      }

      public override void HandleRadsUserKernelLaunched(Process process) { HandlePrePvpNetProcessLaunched(process); }

      public override void HandleLauncherLaunched(Process process) { HandlePrePvpNetProcessLaunched(process); }

      private void HandlePrePvpNetProcessLaunched(Process process)
      {

      }

      public override void HandlePvpNetLaunched(Process process) { throw new NotImplementedException(); }

      public override void HandleGameLaunched(Process process) { throw new NotImplementedException(); }
   }
}
