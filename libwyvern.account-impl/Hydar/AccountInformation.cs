using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class AccountInformation : IPortableObject {
      private string email;
      private string saltedPassword;
      private string name;

      public AccountInformation(string email, string saltedPassword, string name) {
         this.email = email;
         this.saltedPassword = saltedPassword;
         this.name = name;
      }

      public string Email { get { return email; } set { email = value; } }
      public string SaltedPassword { get { return saltedPassword; } set { saltedPassword = value; } }
      public string Name { get { return name; } set { name = value; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, email);
         writer.WriteString(1, saltedPassword);
         writer.WriteString(2, name);
      }

      public void Deserialize(IPofReader reader) {
         email = reader.ReadString(0);
         saltedPassword = reader.ReadString(1);
         name = reader.ReadString(2);
      }
   }
}