using System.Diagnostics;
using Dargon.LeagueOfLegends.Processes;

namespace Dargon.LeagueOfLegends.Session
{
   public interface ILeagueSession
   {
      event LeagueSessionProcessLaunchedHandler ProcessLaunched;
      event LeagueSessionPhaseChangedHandler PhaseChanged;


      Process GetProcessOrNull(LeagueProcessType processType);
   }
}