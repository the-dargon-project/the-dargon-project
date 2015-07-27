using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using ItzWarty;

namespace Dargon.Client.Views.Helpers {
   public class TitleCaseConverter : IValueConverter {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         return value?.ToString().ToTitleCase() ?? "Null";
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
         throw new NotImplementedException();
      }
   }
}
