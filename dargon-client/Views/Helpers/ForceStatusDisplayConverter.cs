using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Dargon.Client.ViewModels;

namespace Dargon.Client.Views.Helpers {
   public class ForceStatusDisplayConverter : IValueConverter {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         ModificationStatus status = (ModificationStatus)value;
         return status == ModificationStatus.Broken ||
                status == ModificationStatus.UpdateAvailable ||
                status == ModificationStatus.Updating;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
         return null;
      }
   }
}
