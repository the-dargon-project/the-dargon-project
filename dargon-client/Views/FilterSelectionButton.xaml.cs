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
using Dargon.LeagueOfLegends.Modifications;

namespace Dargon.Client.Views {
   /// <summary>
   /// Interaction logic for FilterSelectionView.xaml
   /// </summary>
   public partial class FilterSelectionButton : UserControl {
      public FilterSelectionButton() {
         InitializeComponent();
      }

      public static readonly DependencyProperty FilterProperty = DependencyProperty.Register("Filter", typeof(LeagueModificationCategory), typeof(FilterSelectionButton), new FrameworkPropertyMetadata(LeagueModificationCategory.All, FrameworkPropertyMetadataOptions.AffectsRender));
      public LeagueModificationCategory Filter { get { return (LeagueModificationCategory)this.GetValue(FilterProperty); } set { this.SetValue(FilterProperty, value); } }
   }
}
