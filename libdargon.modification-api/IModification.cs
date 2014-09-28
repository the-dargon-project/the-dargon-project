using System;
using Dargon.Game;

namespace Dargon.Modifications
{
   public interface IModification
   {
      Guid LocalGuid { get; }
      GameType GameType { get; }
      string RootPath { get; }
   }
}
