using System.Collections.Generic;
using System.Linq;
using Dargon.PortableObjects;
using Dargon.Trinkets.Components;

namespace Dargon.Trinkets {
   public interface TrinketStartupConfiguration : IPortableObject {
      int TargetProcessId { get; }
      IReadOnlyList<TrinketComponent> Components { get; }
      TComponent GetComponentOrNull<TComponent>() where TComponent : TrinketComponent;
   }

   public class TrinketStartupConfigurationImpl : TrinketStartupConfiguration {
      public TrinketStartupConfigurationImpl() { }

      public TrinketStartupConfigurationImpl(int targetProcessId, IReadOnlyList<TrinketComponent> components) {
         this.TargetProcessId = targetProcessId;
         this.Components = components;
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, TargetProcessId);
         writer.WriteCollection(1, Components, true);
      }

      public void Deserialize(IPofReader reader) {
         TargetProcessId = reader.ReadS32(0);
         Components = reader.ReadCollection<TrinketComponent, List<TrinketComponent>>(1, true);
      }

      public int TargetProcessId { get; private set; }
      public IReadOnlyList<TrinketComponent> Components { get; private set; }
      public TComponent GetComponentOrNull<TComponent>() where TComponent : TrinketComponent {
         return Components.OfType<TComponent>().FirstOrDefault();
      }
   }
}
