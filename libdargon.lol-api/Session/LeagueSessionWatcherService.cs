namespace Dargon.LeagueOfLegends.Session
{
   public interface LeagueSessionWatcherService
   {
      event LeagueSessionCreatedHandler SessionCreated;
   }

   public delegate void LeagueSessionCreatedHandler(LeagueSessionWatcherService service, LeagueSessionCreatedArgs e);

   public class LeagueSessionCreatedArgs
   {
      private readonly ILeagueSession session;

      public LeagueSessionCreatedArgs(ILeagueSession session) {
         this.session = session;
      }

      public ILeagueSession Session { get { return session; } }
   }
}