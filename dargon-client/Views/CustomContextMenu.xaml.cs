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
using System.Windows.Shapes;

namespace Dargon.Client.Views {
   /// <summary>
   /// Interaction logic for CustomContextMenu.xaml
   /// </summary>
   public partial class CustomContextMenu : Window {
      public CustomContextMenu() {
         InitializeComponent();
      }

      protected override void OnDeactivated(EventArgs e) {
         base.OnDeactivated(e);
         this.Hide();
      }
   }
}
