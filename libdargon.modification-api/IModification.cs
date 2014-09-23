using Dargon.Game;

namespace Dargon.Modifications
{
   public interface IModification
   {
      GameType GameType { get; } 
      void Resolve();
      void Compile();
   }
}
