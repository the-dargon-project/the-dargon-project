using Dargon.PortableObjects;

namespace Dargon.Trinkets.Commands {
   public class Command : IPortableObject {
      public Command() { }

      public Command(string type, byte[] data) {
         this.Type = type;
         this.Data = data;
      }

      public string Type { get; private set; }
      public byte[] Data { get; private set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, Type);
         writer.AssignSlot(1, Data);
      }

      public void Deserialize(IPofReader reader) {
         Type = reader.ReadString(0);
         Data = reader.ReadBytes(1);
      }
   }
}