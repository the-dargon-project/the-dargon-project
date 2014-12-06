using Dargon.PortableObjects;

namespace Dargon.Draek.Identities.Dtos {
   public class AuthenticationTokenV1 : IPortableObject {
      public static readonly AuthenticationTokenV1 kInvalidToken = new AuthenticationTokenV1("");

      private string value;

      public AuthenticationTokenV1() { }

      public AuthenticationTokenV1(string value) {
         this.value = value;
      }

      public string Value { get { return value; } }
      public bool IsValid { get { return value.Length > 0; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, value);
      }

      public void Deserialize(IPofReader reader) {
         value = reader.ReadString(0);
      }
   }
}