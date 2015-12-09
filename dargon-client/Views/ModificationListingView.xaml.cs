using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dargon.Client.Views {
   /// <summary>
   /// Interaction logic for ModificationListingView.xaml
   /// </summary>
   public partial class ModificationListingView : UserControl {
      public ModificationListingView() {
         InitializeComponent();
      }

      private void ImportModificationDropdown_Click(object sender, RoutedEventArgs e) {
         var addButton = sender as FrameworkElement;
         if (addButton != null) {
            addButton.ContextMenu.PlacementTarget = addButton;
            addButton.ContextMenu.Placement = PlacementMode.Bottom;
            addButton.ContextMenu.IsOpen = true;
         }
      }
   }
}
