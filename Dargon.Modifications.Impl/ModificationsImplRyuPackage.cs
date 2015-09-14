using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using Dargon.Ryu;

namespace Dargon.Modifications {
   public class ModificationsImplRyuPackage : RyuPackageV1 {
      public ModificationsImplRyuPackage() {
         Singleton<ModificationComponentFactory>();
         Singleton<ModificationLoader, ModificationLoaderImpl>();

         // TODO: Dargon.PortableObjects should handle this.
         Singleton<SlotSourceFactory, SlotSourceFactoryImpl>(RyuTypeFlags.IgnoreDuplicates);
      }
   }
}
