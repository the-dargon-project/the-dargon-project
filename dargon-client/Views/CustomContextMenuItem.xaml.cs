using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dargon.Client.ViewModels;

namespace Dargon.Client.Views {
   /// <summary>
   /// Interaction logic for CustomContextMenuItem.xaml
   /// </summary>
   public partial class CustomContextMenuItem : UserControl {
      public CustomContextMenuItem() {
         InitializeComponent();
      }

      public ContextMenuItemViewModel ViewModel => (ContextMenuItemViewModel)DataContext;

      protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
         base.OnMouseLeftButtonUp(e);
         ViewModel.ClickedCommand.Execute(null);
         Window.GetWindow(this).Close();
      }
   }
}
