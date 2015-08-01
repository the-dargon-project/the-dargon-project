using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dargon.Client.Views.Helpers {
   public class LogicalOrConverter : IMultiValueConverter {
      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
         bool result = false;
         foreach (var value in values.Cast<bool>()) {
            result |= value;
         }
         return result;
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
         return null;
      }
   }
}
