using Dargon.Client.Annotations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dargon.Client.ViewModels {
   public class ContextMenuViewModel : INotifyPropertyChanged {
      public event PropertyChangedEventHandler PropertyChanged;
      private readonly ObservableCollection<ContextMenuItemViewModel> items = new ObservableCollection<ContextMenuItemViewModel>();

      public ObservableCollection<ContextMenuItemViewModel> Items => items;

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
