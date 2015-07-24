using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Dargon.Client.Annotations;
using Dargon.Client.ViewModels.Helpers;

namespace Dargon.Client.ViewModels {
   public class RootViewModel : INotifyPropertyChanged {
      private readonly CollectionViewSource collectionViewSource = new CollectionViewSource();
      private ModificationType modificationTypeFilter = ModificationType.All;
      private ActiveView activeView = ActiveView.ModificationListing;

      public RootViewModel(ObservableCollection<ModificationViewModel> modifications) {
         this.Modifications = modifications;

         collectionViewSource.Source = Modifications;
         collectionViewSource.Filter += (s, e) => {
            e.Accepted = (modificationTypeFilter & ((ModificationViewModel)e.Item).Type) != 0;
         };
         FilteredModifications = collectionViewSource.View;
      }

      public ObservableCollection<ModificationViewModel> Modifications { get; set; }

      public ActiveView ActiveView { get { return activeView; } set { activeView = value; OnPropertyChanged(); } }
      public ModificationType ModificationTypeFilter { get { return modificationTypeFilter; } set { modificationTypeFilter = value; OnPropertyChanged(); } }
      public ICollectionView FilteredModifications { get; set; }

      public ICommand ShowModificationsOfType => new ActionCommand((x) => {
         ActiveView = ActiveView.ModificationListing;
         ModificationTypeFilter = (ModificationType)x;
         collectionViewSource.View.Refresh();
      });

      public ICommand ShowOptions => new ActionCommand((x) => {
         ActiveView = ActiveView.Options;
         collectionViewSource.View.Refresh();
      });

      public event PropertyChangedEventHandler PropertyChanged;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }

   public enum ActiveView {
      ModificationListing,
      Options
   }
}
