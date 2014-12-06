using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects;
using ItzWarty;

namespace Dargon.Wyvern.Accounts {
   public class AccountValidationResult : IPortableObject {
      private bool success;
      private long accountId;
      private string name;

      public AccountValidationResult(bool success) {
         if (success) {
            throw new InvalidOperationException("Must use other ctor overload if validation successful");
         }

         this.success = false;
         this.accountId = -1;
         this.name = string.Empty;
      }

      public AccountValidationResult(bool success, long accountId, string name) {
         name.ThrowIfNull("name");

         this.success = success;
         this.accountId = accountId;
         this.name = name;
      }

      public bool Success { get { return success; } }
      public long AccountId { get { return accountId; } }
      public string AccountName { get { return name; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteBoolean(0, success);
         writer.WriteS64(1, accountId);
         writer.WriteString(2, name);
      }

      public void Deserialize(IPofReader reader) {
         success = reader.ReadBoolean(0);
         accountId = reader.ReadS64(1);
         name = reader.ReadString(2);
      }
   }
}
