using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts {
   public class AccountCreationResult : IPortableObject {
      private long accountId;
      private AccountCreationErrorCodeFlags errorCodeFlags;

      public AccountCreationResult(long accountId, AccountCreationErrorCodeFlags errorCodeFlags) {
         this.accountId = accountId;
         this.errorCodeFlags = errorCodeFlags;
      }

      public long AccountId { get { return accountId; } }
      public AccountCreationErrorCodeFlags ErrorCodeFlags { get { return errorCodeFlags; } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS64(0, accountId);
         writer.WriteS32(1, (int)errorCodeFlags);
      }

      public void Deserialize(IPofReader reader) {
         accountId = reader.ReadS64(0);
         errorCodeFlags = (AccountCreationErrorCodeFlags)reader.ReadS32(1);
      }
   }
}