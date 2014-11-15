using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Game;

namespace Dargon.Modifications
{
   public class ModificationMetadataFactory : IModificationMetadataFactory
   {
      public IModificationMetadata Create(string name, GameType gameType) { return new ModificationMetadata() { Name = name, Targets = new [] { gameType } }; }
   }
}
