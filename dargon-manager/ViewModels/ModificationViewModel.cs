using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dargon.Modifications;

namespace Dargon.Manager.Models {
   public class ModificationViewModel : INotifyPropertyChanged {
      public event PropertyChangedEventHandler PropertyChanged;
      private Modification2 modification;

      public ModificationViewModel(Modification2 modification) {
         this.modification = modification;
      }

      public bool IsEnabled { get { return modification.IsEnabled; } set { modification.IsEnabled = value; OnPropertyChanged(); } }
      public string RepositoryName { get { return modification.RepositoryName; } }
      public string FriendlyName { get { return modification.FriendlyName; } set { modification.FriendlyName = value; OnPropertyChanged(); } }
      public string Description { get { return "This is a test!"; } set { } }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         var handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }

      public void Update(ModificationViewModel viewModel) {
         this.modification = viewModel.modification;
      }
   }
}