using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts {
   public class AccountCreationParameters : IPortableObject {
      private string email;
      private string hashedPassword;

      public AccountCreationParameters() { }

      public AccountCreationParameters(string email, string hashedPassword) {
         this.email = email;
         this.hashedPassword = hashedPassword;
      }

      public string Email { get { return email; } }
      public string HashedPassword { get { return hashedPassword; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, email);
         writer.WriteString(1, hashedPassword);
      }

      public void Deserialize(IPofReader reader) {
         email = reader.ReadString(0);
         hashedPassword = reader.ReadString(1);
      }
   }
}
