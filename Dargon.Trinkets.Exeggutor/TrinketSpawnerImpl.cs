using System.Collections.Generic;
using Dargon.Nest.Eggxecutor;
using Dargon.PortableObjects;
using Dargon.Trinkets.Components;
using ItzWarty.IO;
using ItzWarty.Processes;

namespace Dargon.Trinkets.Spawner {
   public class TrinketSpawnerImpl : TrinketSpawner {
      private const string kTrinketEggName = "trinket";

      private readonly IStreamFactory streamFactory;
      private readonly IPofSerializer pofSerializer;
      private readonly ExeggutorService exeggutorService; 

      public TrinketSpawnerImpl(IStreamFactory streamFactory, IPofSerializer pofSerializer, ExeggutorService exeggutorService) {
         this.streamFactory = streamFactory;
         this.pofSerializer = pofSerializer;
         this.exeggutorService = exeggutorService;
      }

      public void SpawnTrinket(IProcess targetProcess, TrinketSpawnConfiguration trinketSpawnConfiguration) {
         var components = new List<TrinketComponent>();
         if (trinketSpawnConfiguration.IsDebugEnabled) {
            components.Add(new DebugComponent());
         }
         if (trinketSpawnConfiguration.IsFileSystemHookingEnabled) {
            components.Add(new FilesystemComponent(trinketSpawnConfiguration.IsFileSystemOverridingEnabled));
         }
         if (trinketSpawnConfiguration.Name != null) {
            components.Add(new NameComponent(trinketSpawnConfiguration.Name));
         }
         if (trinketSpawnConfiguration.IsLoggingEnabled) {
            components.Add(new VerboseLoggerComponent());
         }
         if (trinketSpawnConfiguration.IsCommandingEnabled) {
            components.Add(new CommandListComponent(trinketSpawnConfiguration.CommandList));
         }
         if (trinketSpawnConfiguration.IsProcessSuspensionEnabled) {
            components.Add(new ProcessSuspensionComponent(trinketSpawnConfiguration.SuspendedProcessNames));
         }

         var targetProcessId = targetProcess.Id;
         var startupConfiguration = new TrinketStartupConfigurationImpl(targetProcessId, components);
         using (var ms = streamFactory.CreateMemoryStream()) {
            pofSerializer.Serialize(ms.Writer, startupConfiguration);
            exeggutorService.SpawnHatchling(
               kTrinketEggName,
               new SpawnConfiguration {
                  Arguments = ms.ToArray(),
                  InstanceName = kTrinketEggName + "_" + targetProcessId,
                  StartFlags = HatchlingStartFlags.StartAsynchronously
               });
         }
      }
   }
}