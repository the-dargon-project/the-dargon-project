using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;

namespace Dargon.Draek.Identities.Hydar {
   public class Identity : IPortableObject {
      private long accountId;
      private string accountName;
      private string email;
      private DateTime expirationTime;

      public Identity(long accountId, string accountName, string email, DateTime expirationTime) {
         this.accountId = accountId;
         this.accountName = accountName;
         this.email = email;
         this.expirationTime = expirationTime;
      }

      public long AccountId { get { return accountId; } }
      public string AccountName { get { return accountName; } }
      public string Email { get { return email; } }
      public DateTime ExpirationTime { get { return expirationTime; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS64(0, accountId);
         writer.WriteString(1, accountName);
         writer.WriteString(2, email);
         writer.WriteS64(3, expirationTime.ToBinary());
      }

      public void Deserialize(IPofReader reader) {
         accountId = reader.ReadS64(0);
         accountName = reader.ReadString(1);
         email = reader.ReadString(2);
         expirationTime = DateTime.FromBinary(reader.ReadS64(3));
      }
   }
}
