using System;
using System.Windows;
using Dargon.Client.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using Dargon.Client.Controllers;
using Dargon.Modifications;

namespace Dargon.Client.Views {
   /// <summary>
   /// Interaction logic for ModificationEntryView.xaml
   /// </summary>
   public partial class ModificationEntryView : UserControl {
      public ModificationEntryView() {
         InitializeComponent();
      }

      public ModificationViewModel ViewModel => (ModificationViewModel)DataContext;
      public ModificationController Controller => ViewModel.Controller;

      /// <summary>
      /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.MouseUp"/> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event. 
      /// </summary>
      /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the mouse button was released.</param>
      protected override void OnMouseUp(MouseButtonEventArgs e) {
         base.OnMouseUp(e);

         switch (e.ChangedButton) {
            case MouseButton.Right:
               var mousePosition = System.Windows.Forms.Cursor.Position;
               var viewModel = new ContextMenuViewModel();
               var currentEnabledState = ViewModel.Modification.GetComponent<EnabledComponent>().IsEnabled;
               viewModel.Items.Add(new ContextMenuItemViewModel { Text = currentEnabledState ? "Disable" : "Enable", ClickedHandler = HandleEnabledClicked });
               viewModel.Items.Add(new ContextMenuItemViewModel { Text = "Configure" });
               viewModel.Items.Add(new ContextMenuItemViewModel { Text = "About" });
               var menu = new CustomContextMenu();
               menu.DataContext = viewModel;
               menu.WindowState = WindowState.Minimized;
               menu.WindowState = WindowState.Normal;
               menu.Left = mousePosition.X;
               menu.Top = mousePosition.Y;
               menu.Show();
               break;
         }
      }

      private void HandleEnabledClicked() {
         ViewModel.IsEnabled = !ViewModel.IsEnabled;
      }
   }
}
