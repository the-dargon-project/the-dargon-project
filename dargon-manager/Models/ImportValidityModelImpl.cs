using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dargon.Manager.Models {
   public class ImportValidityModelImpl : ImportValidityModel {
      public event PropertyChangedEventHandler PropertyChanged;
      private ImportValidity validity = ImportValidity.Undefined;

      public ImportValidityModelImpl() {
      }

      public ImportValidity Validity { get { return validity; } set { validity = value; OnPropertyChanged(); } }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         var handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }
   }
}