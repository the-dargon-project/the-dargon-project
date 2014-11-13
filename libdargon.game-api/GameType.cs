namespace Dargon.Game
{
   public sealed class GameType
   {
      private readonly string name;
      private readonly uint value;

      public static readonly GameType Unknown            = new GameType("", 0);
      public static readonly GameType LeagueOfLegends    = new GameType("league-of-legends", 1);
      public static readonly GameType Any                = new GameType("any", 0xFFFFFFFFU);

      public GameType(string name, uint value)
      {
         this.name = name;
         this.value = value;
      }

      public string Name { get { return name; } }
      public uint Value { get { return value; } }
      public override string ToString() { return name; }

      public static GameType FromString(string s)
      {
         if(s.Equals(Any.Name)) {
            return Any;
         } else if (s.Equals(LeagueOfLegends.Name)) {
            return LeagueOfLegends;
         } else {
            return Unknown;
         }
      }
   }
}
