using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Dargon.UI.Accordion
{
   /// <summary>
   /// Interaction logic for Expandable.xaml
   /// </summary>
   public partial class Expandable : UserControl
   {
      public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(Expandable));

      public Expandable()
      {
         Resources = new ResourceDictionary() {
            Source = new Uri("/libdargon.ui-wpf;component/Accordion/Expandable.xaml", UriKind.RelativeOrAbsolute)
         };
      }

      /// <summary>
      /// The text of our expandable property
      /// </summary>
      public string Text
      {
         get { return (string)GetValue(TextProperty); }
         set { SetValue(TextProperty, value); }
      }
   }
}
