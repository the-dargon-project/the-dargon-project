using ItzWarty.Processes;

namespace Dargon.Trinkets.Spawner {
   public interface TrinketSpawner {
      void SpawnTrinket(IProcess targetProcess, TrinketSpawnConfiguration trinketSpawnConfiguration);
   }
}
