using System.Collections.Generic;

namespace Dargon.Modifications.ThumbnailGenerator.Impl {
   public class EdgeDensityRankComparer : IComparer<float> {
      public int Compare(float x, float y) {
         var result = x.CompareTo(y);
         if (result == 0) {
            return 1;
         } else {
            return result;
         }
      }
   }
}