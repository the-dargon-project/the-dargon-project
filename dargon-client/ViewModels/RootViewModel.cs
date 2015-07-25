using Dargon.Client.Annotations;
using Dargon.Client.Controllers;
using Dargon.Client.ViewModels.Helpers;
using Dargon.Client.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;

namespace Dargon.Client.ViewModels {
   public class RootViewModel : INotifyPropertyChanged {
      private readonly ModificationImportController modificationImportController;
      private readonly MainWindow window;
      private readonly CollectionViewSource collectionViewSource = new CollectionViewSource();
      private ModificationType modificationTypeFilter = ModificationType.All;
      private ActiveView activeView = ActiveView.ModificationListing;

      public RootViewModel(ModificationImportController modificationImportController, MainWindow window, ObservableCollection<ModificationViewModel> modifications) {
         this.modificationImportController = modificationImportController;
         this.window = window;
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

      public ICommand ImportModification => new ActionCommand((x) => {
         var dialog = new FolderBrowserDialog();
         dialog.Description = "Select Modification Folder:";
         dialog.ShowNewFolderButton = false;

         var dialogResult = dialog.ShowDialog();
         if (dialogResult == DialogResult.OK) {
            var modificationPath = dialog.SelectedPath;
            modificationImportController.ShowModificationImportWindowDialog(modificationPath);
         }
      });

      public ICommand FakeSave => new ActionCommand((x) => {
         Mouse.OverrideCursor = Cursors.Wait;
         Thread.Sleep(200);
         Mouse.OverrideCursor = Cursors.Arrow;
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
