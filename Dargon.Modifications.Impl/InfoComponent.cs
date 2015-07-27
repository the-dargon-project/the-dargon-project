using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Dargon.PortableObjects;

namespace Dargon.Modifications.Impl {
   [ModificationComponent(ComponentOrigin.Remote, "INFO")]
   public class InfoComponent : Component {
      private const int kVersion = 0;

      public event PropertyChangedEventHandler PropertyChanged;
      private string name;
      private string[] authors;
      private string website;

      public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }
      public string[] Authors { get { return authors; } set { authors = value; OnPropertyChanged(); } }
      public string Website { get { return website; } set { website = value; OnPropertyChanged(); } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, kVersion);
         writer.WriteString(1, name);
         writer.WriteCollection(2, authors);
         writer.WriteString(3, website);
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadU32(0);
         name = reader.ReadString(1);
         authors = reader.ReadArray<string>(2);
         website = reader.ReadString(3);

         Trace.Assert(version == kVersion);
      }


      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}