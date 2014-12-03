using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dargon.Manager.Models;

namespace Dargon.Manager.ViewModels {
   public class MainWindowViewModel : INotifyPropertyChanged {
      private StatusModel statusModel;

      public MainWindowViewModel(StatusModel statusModel) {
         this.statusModel = statusModel;
      }

      public StatusModel StatusModel { get { return statusModel; } }
      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         var handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}
