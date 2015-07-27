using System.Collections.Generic;

namespace Dargon.Game
{
   public sealed class GameType
   {
      private static readonly Dictionary<uint, GameType> gameTypesByValue = new Dictionary<uint, GameType>();

      private readonly string name;
      private readonly uint value;

      public static readonly GameType Unknown            = new GameType("", 0);
      public static readonly GameType LeagueOfLegends    = new GameType("league-of-legends", 1);
      public static readonly GameType Any                = new GameType("any", 0xFFFFFFFFU);

      private GameType(string name, uint value) {
         this.name = name;
         this.value = value;

         gameTypesByValue.Add(value, this);
      }

      public string Name { get { return name; } }
      public uint Value { get { return value; } }
      public override string ToString() { return name; }

      public static GameType FromValue(uint value) {
         GameType result;
         if (!gameTypesByValue.TryGetValue(value, out result)) {
            result = Unknown;
         }
         return result;
      }
   }
}
