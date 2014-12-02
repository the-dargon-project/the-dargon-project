using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DargonManager
{
   public interface IDMListingViewModel : IEditableObject, INotifyPropertyChanged
   {
      /// <summary>
      /// The name of the entity in our listing view model
      /// </summary>
      string Name { get; set; }

      /// <summary>
      /// The author of the entity 
      /// </summary>
      string Author { get; set; }

      /// <summary>
      /// Whether or not the entity is enabled.
      /// </summary>
      bool IsEnabled { get; set; }
   }
}
