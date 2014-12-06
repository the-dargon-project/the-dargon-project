using Dargon.Hydar;
using Dargon.PortableObjects;

namespace Dargon.Wyvern.Accounts.Hydar {
   public class TryReserveEmailProcessor : IEntryProcessor<string, long, bool>, IPortableObject {
      public bool Process(IEntry<string, long> entry) {
         if (entry.IsPresent && entry.Value != -1) {
            return false;
         } else {
            entry.Value = -1;
            return true;
         }
      }

      public void Serialize(IPofWriter writer) {
         // do nothing
      }

      public void Deserialize(IPofReader reader) {
         // do nothing
      }
   }
}