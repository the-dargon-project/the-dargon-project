using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Ryu;

namespace Dargon.Trinkets.Spawner {
   public class TrinketSpawnerImplRyuPackage : RyuPackageV1 {
      public TrinketSpawnerImplRyuPackage() {
         Singleton<TrinketSpawner, TrinketSpawnerImpl>();
      }
   }
}
