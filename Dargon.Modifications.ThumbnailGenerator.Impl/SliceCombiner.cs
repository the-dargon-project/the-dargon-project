using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AForge.Imaging;
using AForge.Imaging.Filters;
using ItzWarty;

namespace Dargon.Modifications.ThumbnailGenerator {
   public class SliceCombiner {
      private readonly ThumbnailGeneratorUtilities thumbnailGeneratorUtilities;
      private readonly IFilter grayscaleFilter;

      public SliceCombiner(ThumbnailGeneratorUtilities thumbnailGeneratorUtilities, IFilter grayscaleFilter) {
         this.thumbnailGeneratorUtilities = thumbnailGeneratorUtilities;
         this.grayscaleFilter = grayscaleFilter;
      }

      public Bitmap CombineSlices(IReadOnlyList<Bitmap> slices, int slicesPerImage) {
         var sliceImages = slices.Take(50).Shuffle().Take(slicesPerImage).ToArray();
         var orderedSliceImages = sliceImages.OrderBy(sliceImage => {
            var grayscaleImage = grayscaleFilter.Apply(sliceImage);
            return new HorizontalIntensityStatistics(grayscaleImage).Gray.TotalCount;
         }).ToList();
         var shuffledSliceImages = thumbnailGeneratorUtilities.HalfShuffle(orderedSliceImages);

         var sliceSize = sliceImages.First().Size;
         var resultImage = new Bitmap(sliceSize.Width * slicesPerImage, sliceSize.Height);
         using (var g = Graphics.FromImage(resultImage)) {
            for (var i = 0; i < slicesPerImage; i++) {
               g.DrawImage(
                  shuffledSliceImages[i],
                  new Rectangle(i * sliceSize.Width, 0, sliceSize.Width, sliceSize.Height),
                  new Rectangle(0, 0, sliceSize.Width, sliceSize.Height),
                  GraphicsUnit.Pixel);
            }
         }
         return resultImage;
      }
   }
}