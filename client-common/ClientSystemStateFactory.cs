using ItzWarty.IO;
using System.IO;
using System.Reflection;

namespace Dargon {
   public class ClientSystemStateFactory {
      private const string kSystemStateConfigurationDirectoryName = "system-state";

      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IClientConfiguration clientConfiguration;

      public ClientSystemStateFactory(IFileSystemProxy fileSystemProxy, IClientConfiguration clientConfiguration) {
         this.fileSystemProxy = fileSystemProxy;
         this.clientConfiguration = clientConfiguration;
      }

      public SystemState Create() {
         SystemState systemState = CreateFileBackedSystemState();

         SystemState fallbackSystemState;
         if (TryLoadJsonSystemState("local-toggles.json", out fallbackSystemState)) {
            systemState = new PrimaryFallbackSystemStateImpl(systemState, fallbackSystemState);
         }

         if (TryLoadJsonSystemState("remote-toggles.json", out fallbackSystemState)) {
            systemState = new PrimaryFallbackSystemStateImpl(systemState, fallbackSystemState);
         }

         return systemState;
      }

      private bool TryLoadJsonSystemState(string name, out SystemState systemState) {
         var nestHostExecutablePath = Assembly.GetEntryAssembly().Location;
         var nestHostDirectoryInfo = new FileInfo(nestHostExecutablePath).Directory;
         var nestDirectoryInfo = nestHostDirectoryInfo.Parent;
         var jsonTogglesPath = Path.Combine(nestDirectoryInfo.FullName, name);
         if (File.Exists(jsonTogglesPath)) {
            systemState = JsonReadOnlySystemStateImpl.FromFile(jsonTogglesPath);
            return true;
         } else {
            systemState = null;
            return false;
         }
      }

      private SystemState CreateFileBackedSystemState() {
         var systemStateDirectoryPath = Path.Combine(clientConfiguration.ConfigurationDirectoryPath, kSystemStateConfigurationDirectoryName);
         var fileBackedSystemState = new FileBackedSystemStateImpl(fileSystemProxy, systemStateDirectoryPath);
         return fileBackedSystemState;
      }
   }
}
