using Dargon.Hydar;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class TryInitializeAccountNameProcessor : IEntryProcessor<long, AccountInformation, bool>, IPortableObject {
      private string newName;

      public TryInitializeAccountNameProcessor() { }

      public TryInitializeAccountNameProcessor(string newName) {
         this.newName = newName;
      }

      public bool Process(IEntry<long, AccountInformation> entry) {
         if (!entry.IsPresent || entry.Value.Name != null) {
            return false;
         } else {
            entry.Value.Name = newName;
            entry.FlagAsDirty();
            return true;
         }
      }

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, newName);
      }

      public void Deserialize(IPofReader reader) {
         newName = reader.ReadString(0);
      }
   }
}