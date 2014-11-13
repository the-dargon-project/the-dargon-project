using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Modifications
{
   public interface IModificationLoader
   {
      IModification Load(string name, string path);
   }
}
