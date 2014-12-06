using Dargon.Hydar;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class TryAssignAccountIdProcessor : IEntryProcessor<string, long, bool>, IPortableObject {
      private long accountId;

      public TryAssignAccountIdProcessor() { }

      public TryAssignAccountIdProcessor(long accountId) {
         this.accountId = accountId;
      }

      public bool Process(IEntry<string, long> entry) {
         if (!entry.IsPresent) {
            return false;
         } else {
            entry.Value = accountId;
            return true;
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteS64(0, accountId);
      }

      public void Deserialize(IPofReader reader) {
         accountId = reader.ReadS64(0);
      }
   }
}