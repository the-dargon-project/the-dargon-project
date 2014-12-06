using Dargon.PortableObjects;

namespace Dargon.Draek.Identities {
   public class ApplicationIdentifierV1 : IPortableObject {
      private string name;

      public ApplicationIdentifierV1(string name) {
         this.name = name;
      }

      public string Name { get { return name; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, name);
      }

      public void Deserialize(IPofReader reader) {
         name = reader.ReadString(0);
      }
   }
}