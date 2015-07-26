using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Game;

namespace Dargon.Modifications
{
   public interface IModificationMetadataFactory
   {
      IModificationMetadata Create(string friendlyName, GameType gameType);
   }
}
