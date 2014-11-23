using System.Diagnostics;
using Dargon.LeagueOfLegends.Processes;
using Dargon.Processes;
using ItzWarty.Processes;

namespace Dargon.LeagueOfLegends.Session
{
   public interface ILeagueSession
   {
      event LeagueSessionProcessLaunchedHandler ProcessLaunched;
      event LeagueSessionPhaseChangedHandler PhaseChanged;

      IProcess GetProcessOrNull(LeagueProcessType processType);
   }
}