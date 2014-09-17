using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon
{
   public class CommonResources
   {
      public static readonly Resources2011Collection Resources2011 = new Resources2011Collection();

      public class Resources2011Collection
      {
         public Icon Icon { get { return Properties.Resources.Icon; } }
      }
   }
}
