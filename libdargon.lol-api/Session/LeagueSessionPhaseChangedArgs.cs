namespace Dargon.LeagueOfLegends.Session
{
   public class LeagueSessionPhaseChangedArgs
   {
      private readonly LeagueSessionPhase previous;
      private readonly LeagueSessionPhase current;

      public LeagueSessionPhaseChangedArgs(LeagueSessionPhase previous, LeagueSessionPhase current)
      {
         this.previous = previous;
         this.current = current;
      }

      public LeagueSessionPhase Previous { get { return previous; } }
      public LeagueSessionPhase Current { get { return current; } }
   }

   public delegate void LeagueSessionPhaseChangedHandler(ILeagueSession session, LeagueSessionPhaseChangedArgs e);
}