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
using Dargon.Manager;

namespace DargonManager
{
   /// <summary>
   /// Interaction logic for ModListingJr.xaml
   /// </summary>
   public partial class ModListingJr : ContentControl
   {
      /// <summary>
      /// View Model of our Modification Listing.  Useful for getting a reference to Dargon.
      /// </summary>
      public DMViewModelBase ViewModelBase { get; set; }

      public ModListingJr()
      {
         InitializeComponent();
         DataContextChanged += ModListing_DataContextChanged;
      }

      /// <summary>
      /// When the data context of the mod listing changes, we ensure that it is a DMViewModel 
      /// object and store a reference to it for future use (drag + drop).
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void ModListing_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         ViewModelBase = (DMViewModelBase)e.NewValue;
      }
   }
}
