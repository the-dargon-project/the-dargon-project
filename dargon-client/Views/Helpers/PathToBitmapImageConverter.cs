using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dargon.Client.Views.Helpers {
   public class PathToBitmapImageConverter : IValueConverter {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         var imagePath = (string)value;
         if (imagePath == null) {
            return null;
         } else {
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = new MemoryStream(File.ReadAllBytes(imagePath));
            source.EndInit();
            return source;
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
         return null;
      }
   }
}
