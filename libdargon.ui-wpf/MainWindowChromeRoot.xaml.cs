using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Dargon.UI.WPF {
   [ContentProperty("Body")]
   public partial class MainWindowChromeRoot : UserControl {
      public MainWindowChromeRoot() {
         InitializeComponent();
      }

      public object Body {
         get { return GetValue(BodyProperty); }
         set { SetValue(BodyProperty, value); }
      }

      public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(nameof(Body), typeof(object), typeof(MainWindowChromeRoot), null);

      public object ToolStrip {
         get { return GetValue(ToolStripProperty); }
         set { SetValue(ToolStripProperty, value); }
      }

      public static readonly DependencyProperty ToolStripProperty = DependencyProperty.Register(nameof(ToolStrip), typeof(object), typeof(MainWindowChromeRoot), null);
   }
}
