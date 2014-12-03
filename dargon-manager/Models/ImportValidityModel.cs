using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Manager.Models {
   public interface ImportValidityModel : INotifyPropertyChanged {
      ImportValidity Validity { get; }
   }
}
