using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dargon.Modifications;

namespace Dargon.Manager.Models {
   public class ModificationViewModel : INotifyPropertyChanged {
      public event PropertyChangedEventHandler PropertyChanged;
      private IModification modification;

      public ModificationViewModel(IModification modification) {
         this.modification = modification;
      }

      public bool IsEnabled { get { return false; } set { } }
      public string RepositoryName { get { return modification.RepositoryName; } }
      public string FriendlyName { get { return GetFriendlyName(); } set { } }
      public string Description { get { return "This is a test!"; } set { } }

      private string GetFriendlyName() {
         var metadataName = modification.Metadata.Name;
         var repositoryName = modification.RepositoryName;
         if (string.IsNullOrWhiteSpace(metadataName)) {
            return repositoryName;
         } else {
            return metadataName;
         }
      }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
         var handler = PropertyChanged;
         if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
      }

      public void Update(ModificationViewModel viewModel) {
         this.modification = viewModel.modification;
      }
   }
}