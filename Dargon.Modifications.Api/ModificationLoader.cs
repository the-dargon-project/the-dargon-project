using System.Collections.Generic;

namespace Dargon.Modifications {
   public interface ModificationLoader {
      IReadOnlyList<Modification> EnumerateModifications();
      Modification FromPath(string path);
   }
}