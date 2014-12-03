using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dargon.Manager.Models {
   public class StatusModelImpl : StatusModel {
      public event PropertyChangedEventHandler PropertyChanged;
      private string status = "Hello.";

      public string Status {
         get { return status; }
         set {
            status = value;
            OnPropertyChanged();
         }
      }

      private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         var handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}