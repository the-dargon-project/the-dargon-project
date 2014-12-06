using Dargon.PortableObjects;

namespace Dargon.Draek.Identities {
   public class AuthenticationCredentialsV1 : IPortableObject {
      private string email;
      private string hashedPassword;

      public AuthenticationCredentialsV1(string email, string hashedPassword) {
         this.email = email;
         this.hashedPassword = hashedPassword;
      }

      public string Email { get { return email; } }
      public string HashedPassword { get { return hashedPassword; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, email);
         writer.WriteString(0, hashedPassword);
      }

      public void Deserialize(IPofReader reader) {
         email = reader.ReadString(0);
         hashedPassword = reader.ReadString(1);
      }
   }
}
