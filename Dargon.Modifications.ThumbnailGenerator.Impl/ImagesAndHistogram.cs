using System.Drawing;
using AForge.Math;

namespace Dargon.Modifications.ThumbnailGenerator {
   public class ImagesAndHistogram {
      public Bitmap OriginalImage { get; set; }
      public Bitmap EdgeImage { get; set; }
      public Histogram EdgeDensityHistogram { get; set; }
      public double Rating { get; set; }
      public Bitmap SliceImage { get; set; }
      public Bitmap SliceImageResized { get; set; }
   }
}