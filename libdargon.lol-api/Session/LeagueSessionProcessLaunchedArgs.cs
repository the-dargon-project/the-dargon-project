using System.Diagnostics;
using Dargon.LeagueOfLegends.Processes;

namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSessionProcessLaunchedArgs
   {
      private readonly LeagueProcessType type;
      private readonly IProcess process;

      public LeagueSessionProcessLaunchedArgs(LeagueProcessType type, IProcess process)
      {
         this.type = type;
         this.process = process;
      }

      public LeagueProcessType Type { get { return type; } }
      public IProcess Process { get { return process; } }
   }

   public delegate void LeagueSessionProcessLaunchedHandler(ILeagueSession session, LeagueSessionProcessLaunchedArgs e);
}