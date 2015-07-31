using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Modifications.ThumbnailGenerator {
   public class ThumbnailGeneratorApiPofContext : PofContext {
      private const int kBasePofId = 14000;

      public ThumbnailGeneratorApiPofContext() {
         RegisterPortableObjectType(kBasePofId + 0, typeof(ThumbnailGenerationParameters));
      }
   }
}
