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
         ModificationEntryStatus status = (ModificationEntryStatus)value;
         return status == ModificationEntryStatus.Broken ||
                status == ModificationEntryStatus.UpdateAvailable ||
                status == ModificationEntryStatus.Updating;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
         return null;
      }
   }
}
