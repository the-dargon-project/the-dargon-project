using Dargon.PortableObjects;

namespace Dargon.Trinkets {
   public interface TrinketStartupConfiguration : IPortableObject {
      int TargetProcessId { get; }
      string TrinketDllPath { get; }
   }

   public class TrinketStartupConfigurationImpl : TrinketStartupConfiguration {
      public TrinketStartupConfigurationImpl() { }

      public TrinketStartupConfigurationImpl(int targetProcessId, string trinketDllPath) {
         this.TargetProcessId = targetProcessId;
         this.TrinketDllPath = trinketDllPath;
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, TargetProcessId);
         writer.WriteString(1, TrinketDllPath);
      }

      public void Deserialize(IPofReader reader) {
         TargetProcessId = reader.ReadS32(0);
         TrinketDllPath = reader.ReadString(1);
      }

      public int TargetProcessId { get; private set; }
      public string TrinketDllPath { get; private set; }
   }
}
