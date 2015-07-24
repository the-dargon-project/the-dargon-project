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
   /// Interaction logic for FilterSelectionView.xaml
   /// </summary>
   public partial class FilterSelectionButton : UserControl {
      public FilterSelectionButton() {
         InitializeComponent();
      }

      public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(ModificationType), typeof(FilterSelectionButton), new FrameworkPropertyMetadata(ModificationType.All, FrameworkPropertyMetadataOptions.AffectsRender));
      public ModificationType Filter { get { return (ModificationType)this.GetValue(FilterProperty); } set { this.SetValue(FilterProperty, value); } }
   }
}
