using Dargon.Hydar;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class AccountCreationProcessor : IEntryProcessor<long, AccountInformation, bool>, IPortableObject {
      private string email;
      private string saltedPassword;

      public AccountCreationProcessor() { }

      public AccountCreationProcessor(string email, string saltedPassword) {
         this.email = email;
         this.saltedPassword = saltedPassword;
      }

      public bool Process(IEntry<long, AccountInformation> entry) {
         if (entry.IsPresent) {
            return false;
         } else {
            entry.Value = new AccountInformation(email, saltedPassword, null);
            return true;
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, email);
         writer.WriteString(1, saltedPassword);
      }

      public void Deserialize(IPofReader reader) {
         email = reader.ReadString(0);
         saltedPassword = reader.ReadString(1);
      }
   }
}