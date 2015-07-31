using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Math;

namespace Dargon.Modifications.ThumbnailGenerator.Impl {
   public class SliceRatingCalculator {
      public double ComputeRating(Bitmap originalImage, Histogram edgeDensityHistogram, int resultWidth, int resultHeight) {
         var wingSpreadFactor = edgeDensityHistogram.GetRange(0.2).Length / (double)edgeDensityHistogram.GetRange(0.6).Length;
         var focusFactor = edgeDensityHistogram.GetRange(0.5).Length / (double)originalImage.Width;
         var widthFactor = originalImage.Width < resultWidth * 2 ? 5 : 1;
         var heightFactor = originalImage.Height < resultHeight * 2 ? 5 : 1;
         var brightnessFactor = Math.Pow(originalImage.Width * originalImage.Height / (double)edgeDensityHistogram.TotalCount, 1.5);
         return wingSpreadFactor * focusFactor * widthFactor * heightFactor * brightnessFactor;
      }
   }
}
