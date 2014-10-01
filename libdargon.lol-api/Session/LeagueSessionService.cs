namespace Dargon.LeagueOfLegends.Session
{
   public interface LeagueSessionService
   {
      event LeagueSessionCreatedHandler SessionCreated;

      ILeagueSession GetProcessSessionOrNull(int processId);
   }
}