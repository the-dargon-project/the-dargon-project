using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Dargon.Game;
using Dargon.PortableObjects;
using ItzWarty;

namespace Dargon.Modifications {
   [ModificationComponent(ComponentOrigin.Remote, "INFO")]
   public class InfoComponent : Component {
      private const int kVersion = 0;

      public event PropertyChangedEventHandler PropertyChanged;
      private Guid id;
      private string name = "";
      private string[] authors = new string[0];
      private string website = "";
      private GameType[] targets = new GameType[0];

      public Guid Id { get { return id; } set { id = value; OnPropertyChanged(); } }
      public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }
      public string[] Authors { get { return authors; } set { authors = value; OnPropertyChanged(); } }
      public string Website { get { return website; } set { website = value; OnPropertyChanged(); } }
      public GameType[] Targets { get { return targets; } set { targets = value; OnPropertyChanged(); } }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, kVersion);
         writer.WriteGuid(1, id);
         writer.WriteString(2, name);
         writer.WriteCollection(3, authors);
         writer.WriteString(4, website);
         writer.WriteCollection(5, targets.Select(target => target.Value));
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadU32(0);
         id = reader.ReadGuid(1);
         name = reader.ReadString(2);
         authors = reader.ReadArray<string>(3);
         website = reader.ReadString(4);
         Targets = reader.ReadArray<uint>(5).Select(GameType.FromValue).ToArray();

         Trace.Assert(version == kVersion);
      }


      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}