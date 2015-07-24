using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dargon.UI.WPF {
   public class MainWindow : Window {
      private readonly MainWindowChromeRoot chromeRoot;
      private bool firstRender = true;

      public MainWindow() {
         chromeRoot = new MainWindowChromeRoot();
         chromeRoot.Body = new TextBlock() { Text = "Derp" };
         chromeRoot.ToolStrip = new TextBlock() { Text = "Derp" };

         this.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(@"/Dargon.UI.WPF;component/LightTheme.xaml", UriKind.Relative) });
         this.Content = chromeRoot;
      }

      protected override void OnRender(DrawingContext drawingContext) {
         if (firstRender) {
            firstRender = false;
            chromeRoot.Body = Body;
            chromeRoot.ToolStrip = ToolStrip;
         }

         base.OnRender(drawingContext);
      }

      protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
         base.OnPropertyChanged(e);

         switch (e.Property.Name) {
            case "Body":
               chromeRoot.Body = e.NewValue;
               break;
            case "ToolStrip":
               chromeRoot.ToolStrip = e.NewValue;
               break;
         }
      }

      public object Body {
         get { return GetValue(BodyProperty); }
         set { SetValue(BodyProperty, value); }
      }

      public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(nameof(Body), typeof(object), typeof(MainWindow), null);

      public object ToolStrip {
         get { return GetValue(ToolStripProperty); }
         set { SetValue(ToolStripProperty, value); }
      }

      public static readonly DependencyProperty ToolStripProperty = DependencyProperty.Register(nameof(ToolStrip), typeof(object), typeof(MainWindow), null);
   }
}
