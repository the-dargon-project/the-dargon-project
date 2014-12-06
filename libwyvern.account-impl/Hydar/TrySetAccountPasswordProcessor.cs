using Dargon.Hydar;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class TrySetAccountPasswordProcessor : IEntryProcessor<long, AccountInformation, bool>, IPortableObject {
      private string newSaltedPassword;

      public TrySetAccountPasswordProcessor() { }

      public TrySetAccountPasswordProcessor(string newSaltedPassword) {
         this.newSaltedPassword = newSaltedPassword;
      }

      public bool Process(IEntry<long, AccountInformation> entry) {
         if (!entry.IsPresent) {
            return false;
         } else {
            entry.Value.SaltedPassword = newSaltedPassword;
            entry.FlagAsDirty();
            return true;
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, newSaltedPassword);
      }

      public void Deserialize(IPofReader reader) {
         newSaltedPassword = reader.ReadString(0);
      }
   }
}