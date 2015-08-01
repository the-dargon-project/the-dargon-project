using Dargon.Client.Annotations;
using Dargon.Modifications;
using ItzWarty;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dargon.Client.Controllers;
using Dargon.LeagueOfLegends.Modifications;
using Dargon.Patcher;

namespace Dargon.Client.ViewModels {
   public class ModificationViewModel : INotifyPropertyChanged {
      public event PropertyChangedEventHandler PropertyChanged;

      private ModificationController controller;

      private Modification modification;
      private InfoComponent info;
      private LeagueMetadataComponent leagueComponent;
      private ModificationStatus status;
      private double statusProgress;

      public string RepositoryPath => modification.RepositoryPath;
      public string RepositoryName => modification.RepositoryName;
      public string Name { get { return info.Name; } set { info.Name = value; OnPropertyChanged(); } }
      public string[] Authors { get { return info.Authors; } set { info.Authors = value; OnPropertyChanged(); } }
      public string Author => Authors.Join(", ");
      public ModificationStatus Status { get { return status; } set { status = value; OnPropertyChanged(); } }
      public double StatusProgress { get { return statusProgress; } set { statusProgress = value; OnPropertyChanged(); } }
      public LeagueModificationCategory Category => leagueComponent.Category; // LeagueModificationCategory.FromString(File.ReadAllText(new LocalRepository(RepositoryPath).GetMetadataFilePath("CATEGORY"), Encoding.UTF8));
      public Modification Modification => modification;

      public void SetModification(Modification newModificationValue) {
         if (this.modification != null) {
            this.info.PropertyChanged -= HandleComponentPropertyChanged;
            this.leagueComponent.PropertyChanged -= HandleComponentPropertyChanged;
         }

         this.modification = newModificationValue;

         this.info = newModificationValue.GetComponent<InfoComponent>();
         this.leagueComponent = newModificationValue.GetComponent<LeagueMetadataComponent>();

         info.PropertyChanged += HandleComponentPropertyChanged;
         leagueComponent.PropertyChanged += HandleComponentPropertyChanged;

         OnPropertyChanged(nameof(Modification));
      }

      public void SetController(ModificationController controller) {
         this.controller = controller;
      }

      
      private void HandleComponentPropertyChanged(object sender, PropertyChangedEventArgs e) {
         OnPropertyChanged(e.PropertyName);
      }

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
      UpdateAvailable = 0x08,
      Updating = 0x10
   }
}
