namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSessionCreatedArgs
   {
      private readonly ILeagueSession session;

      public LeagueSessionCreatedArgs(ILeagueSession session) {
         this.session = session;
      }

      public ILeagueSession Session { get { return session; } }
   }

   public delegate void LeagueSessionCreatedHandler(LeagueSessionService service, LeagueSessionCreatedArgs e);
}