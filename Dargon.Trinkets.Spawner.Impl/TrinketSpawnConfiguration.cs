using Dargon.Trinkets.Commands;
using ItzWarty.Collections;

namespace Dargon.Trinkets.Spawner {
   public interface TrinketSpawnConfiguration {
      bool IsDebugEnabled { get; set; }
      bool IsFileSystemHookingEnabled { get; set; }
      bool IsFileSystemOverridingEnabled { get; set; }
      string Name { get; set; }
      bool IsLoggingEnabled { get; set; }
      bool IsCommandingEnabled { get; set; }
      CommandList CommandList { get; set; }
      bool IsProcessSuspensionEnabled { get; set; }
      IReadOnlySet<string> SuspendedProcessNames { get; set; }
   }
}