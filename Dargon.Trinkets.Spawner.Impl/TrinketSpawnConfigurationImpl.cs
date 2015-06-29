using Dargon.Trinkets.Commands;
using ItzWarty.Collections;

namespace Dargon.Trinkets.Spawner {
   public class TrinketSpawnConfigurationImpl : TrinketSpawnConfiguration {
      public bool IsDebugEnabled { get; set; } = true;
      public bool IsFileSystemHookingEnabled { get; set; } = false;
      public bool IsFileSystemOverridingEnabled { get; set; } = false;
      public string Name { get; set; } = null;
      public bool IsLoggingEnabled { get; set; } = true;
      public bool IsCommandingEnabled { get; set; } = false;
      public CommandList CommandList { get; set; } = null;
      public bool IsProcessSuspensionEnabled { get; set; }
      public IReadOnlySet<string> SuspendedProcessNames { get; set; }
   }
}