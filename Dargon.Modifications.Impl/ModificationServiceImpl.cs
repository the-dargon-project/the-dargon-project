using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.PortableObjects.Streams;

namespace Dargon.Modifications.Impl {
   public class ModificationServiceImpl {
      private readonly string repositoriesPath;

      public ModificationServiceImpl(string repositoriesPath) {
         this.repositoriesPath = repositoriesPath;
         ObservableCollection<int> x;
      }
   }
}
