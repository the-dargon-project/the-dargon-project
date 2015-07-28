using Dargon.Client.Annotations;
using Dargon.Modifications;
using ItzWarty;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Patcher;

namespace Dargon.Client.ViewModels {
   public class ModificationViewModel : INotifyPropertyChanged {
      private readonly Modification modification;
      private readonly InfoComponent info;
      private readonly LeagueMetadataComponent leagueComponent;
      public event PropertyChangedEventHandler PropertyChanged;

      public ModificationViewModel(Modification modification) {
         this.modification = modification;
         this.info = modification.GetComponent<InfoComponent>();
         info.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
         this.leagueComponent = modification.GetComponent<LeagueMetadataComponent>();
         leagueComponent.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
      }

      public string RepositoryPath => modification.RepositoryPath;
      public string RepositoryName => modification.RepositoryName;
      public string Name { get { return info.Name; } set { info.Name = value; OnPropertyChanged(); } }
      public string[] Authors { get { return info.Authors; } set { info.Authors = value; OnPropertyChanged(); } }
      public string Author => Authors.Join(", ");
      public ModificationStatus Status { get { return ModificationStatus.Enabled; } set { throw new NotImplementedException(); } }
      public LeagueModificationCategory Category => leagueComponent.Category; // LeagueModificationCategory.FromString(File.ReadAllText(new LocalRepository(RepositoryPath).GetMetadataFilePath("CATEGORY"), Encoding.UTF8));

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }

   [Flags]
   public enum ModificationStatus {
      Enabled = 0x01,
      Disabled = 0x02,
      Broken = 0x04,
      UpdateAvailable = 0x08
   }
}
