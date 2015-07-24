using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dargon.Manager {
   /// <summary>
   /// Interaction logic for ModListingJr.xaml
   /// </summary>
   public partial class MainModificationListing : ContentControl {
      public MainModificationListing() {
         InitializeComponent();
      }

      /// <summary>
      /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing. 
      /// </summary>
      /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
      protected override void OnRender(DrawingContext drawingContext) {
         base.OnRender(drawingContext);
      }
   }
}
