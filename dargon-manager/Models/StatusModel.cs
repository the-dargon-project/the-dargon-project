using System.ComponentModel;

namespace Dargon.Manager.Models {
   public interface StatusModel : INotifyPropertyChanged {
      string Status { get; }
   }
}
