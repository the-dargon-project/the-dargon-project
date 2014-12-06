using System.Collections.Generic;
using Dargon.PortableObjects;

namespace Dargon.Draek.Identities.Dtos {
   public class IdentityV1 : IPortableObject {
      private long accountId;
      private string accountName;
      private string email;

      public IdentityV1(long accountId, string accountName, string email) {
         this.accountId = accountId;
         this.accountName = accountName;
         this.email = email;
      }

      public long AccountId { get { return accountId; } }
      public string AccountName { get { return accountName; } }
      public string Email { get { return email; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS64(0, accountId);
         writer.WriteString(1, accountName);
         writer.WriteString(2, email);
      }

      public void Deserialize(IPofReader reader) {
         accountId = reader.ReadS64(0);
         accountName = reader.ReadString(1);
         email = reader.ReadString(2);
      }
   }
}
