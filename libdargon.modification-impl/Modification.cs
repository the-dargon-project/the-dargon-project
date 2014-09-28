using System;
using Dargon.Game;

namespace Dargon.Modifications
{
   public class Modification : IModification
   {
      private readonly GameType gameType;
      private readonly Guid localGuid;
      private readonly string rootPath;

      public Modification(GameType gameType, Guid localGuid, string rootPath)
      {
         this.gameType = gameType;
         this.localGuid = localGuid;
         this.rootPath = rootPath;
      }

      public GameType GameType { get { return gameType; } }
      public Guid LocalGuid { get { return localGuid; } }
      public string RootPath { get { return rootPath; } }
   }
}
