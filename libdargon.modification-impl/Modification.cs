using Dargon.Game;

namespace Dargon.Modifications
{
   public class Modification : IModification
   {
      private readonly string rootPath;
      private readonly GameType gameType;

      public Modification(string rootPath, GameType gameType)
      {
         this.rootPath = rootPath;
         this.gameType = gameType;
      }

      public GameType GameType { get { return gameType; } }

      public void Resolve() { }

      public void Compile() { }
   }
}
