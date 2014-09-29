namespace Dargon.LeagueOfLegends.Session
{
   public interface ILeagueSession
   {
      event LeagueSessionProcessLaunchedHandler ProcessLaunched;
      event LeagueSessionPhaseChangedHandler PhaseChanged;
   }
}