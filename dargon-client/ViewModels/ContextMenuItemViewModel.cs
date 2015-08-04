using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dargon.Client.Annotations;
using Dargon.Client.ViewModels.Helpers;

namespace Dargon.Client.ViewModels {
   public class ContextMenuItemViewModel : INotifyPropertyChanged {
      public event PropertyChangedEventHandler PropertyChanged;
      private string text;

      public Action ClickedHandler { get; set; }
      public string Text { get { return text; } set { text = value; OnPropertyChanged(); } }
      public ICommand ClickedCommand => new ActionCommand((x) => {
         ClickedHandler?.Invoke();
      });

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
