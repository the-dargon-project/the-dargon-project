using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon {
   public class TemporaryFilesImplRyuPackage : RyuPackageV1 {
      public TemporaryFilesImplRyuPackage() {
         LocalService<TemporaryFileService, TemporaryFileServiceImpl>();
      }
   }
}
