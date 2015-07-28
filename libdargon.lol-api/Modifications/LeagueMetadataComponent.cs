using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.Modifications;
using Dargon.PortableObjects;
using NLog;
using Component = Dargon.Modifications.Component;

namespace Dargon.LeagueOfLegends.Modifications {
   [ModificationComponent(ComponentOrigin.Remote, "LEAGUE_METADATA")]
   public class LeagueMetadataComponent : Component {
      private const int kVersion = 1;

      public event PropertyChangedEventHandler PropertyChanged;
      private LeagueModificationCategory category = LeagueModificationCategory.Unknown;

      public LeagueModificationCategory Category { get { return category; } set { category = value; OnPropertyChanged(); } }

      public void Serialize(IPofWriter writer) {
         writer.WriteS32(0, kVersion);
         writer.WriteU32(1, category.Value);
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadS32(0);
         category = LeagueModificationCategory.FromValue(reader.ReadU32(1));

         Trace.Assert(version == kVersion, $"Unexpected {nameof(LeagueMetadataComponent)} kVersion {version} expected {kVersion}!");
      }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
