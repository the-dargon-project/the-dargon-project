using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DargonManager.SampleData
{
   public class SampleViewModel
   {
      public SampleViewModel()
      {
      }

      public SampleViewModel(List<SampleModification> mods)
      {
         ListedEntities = new ObservableCollection<SampleModification>(mods);
      }

      public ObservableCollection<SampleModification> ListedEntities { get; set; }
   }

   public class SampleModification
   {
      public string Name { get; set; }
      public string Author { get; set; }
      public bool IsEnabled { get; set; }
      public string Type { get; set; }
   }
}
