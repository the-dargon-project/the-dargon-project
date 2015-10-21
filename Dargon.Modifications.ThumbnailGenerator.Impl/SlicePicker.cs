using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace Dargon.Modifications.ThumbnailGenerator {
   public class SlicePicker {
      private readonly IFilter grayscaleFilter;
      private readonly IInPlaceFilter edgeDetectionFilter;
      private readonly SliceRatingCalculator ratingCalculator;
      private readonly ThumbnailGeneratorUtilities thumbnailGeneratorUtilities;

      public SlicePicker(IFilter grayscaleFilter, IInPlaceFilter edgeDetectionFilter, SliceRatingCalculator ratingCalculator, ThumbnailGeneratorUtilities thumbnailGeneratorUtilities) {
         this.grayscaleFilter = grayscaleFilter;
         this.edgeDetectionFilter = edgeDetectionFilter;
         this.ratingCalculator = ratingCalculator;
         this.thumbnailGeneratorUtilities = thumbnailGeneratorUtilities;
      }

      public List<Bitmap> PickSlices(IReadOnlyList<Bitmap> inputImages, Size sliceSize) {
         var edgeImages = inputImages.Select(grayscaleFilter.Apply).ToList();
         edgeImages.ForEach(edgeDetectionFilter.ApplyInPlace);

         var edgeDensityHistograms = edgeImages.Select(x => new HorizontalIntensityStatistics(x).Gray).ToList();

         var imagesAndHistograms = Enumerable.Range(0, inputImages.Count).Select(index => {
            return new ImagesAndHistogram {
               OriginalImage = inputImages[index],
               EdgeImage = edgeImages[index],
               EdgeDensityHistogram = edgeDensityHistograms[index],
               Rating = ratingCalculator.ComputeRating(inputImages[index], edgeDensityHistograms[index], sliceSize.Width, sliceSize.Height)
            };
         }).OrderBy(x => x.Rating).ToList();

         var sliceAspect = sliceSize.Width / (double)sliceSize.Height;
         foreach (var imagesAndHistogram in imagesAndHistograms) {
            var desiredWidth = (int)(imagesAndHistogram.OriginalImage.Height * sliceAspect);
            if (desiredWidth > imagesAndHistogram.OriginalImage.Width) {
               continue;
            }
            var range = thumbnailGeneratorUtilities.GetRangeOfWidth(imagesAndHistogram.EdgeDensityHistogram, desiredWidth);

            var horizontalCrop = new Crop(new Rectangle(range.Min, 0, range.Max - range.Min, imagesAndHistogram.OriginalImage.Height)).Apply(imagesAndHistogram.OriginalImage);
            var horizontalCrop24bpp = horizontalCrop.Clone(new Rectangle(Point.Empty, horizontalCrop.Size), PixelFormat.Format24bppRgb);
            imagesAndHistogram.SliceImage = horizontalCrop24bpp;
            var resizer = new ResizeBicubic(sliceSize.Width, sliceSize.Height);
            imagesAndHistogram.SliceImageResized = resizer.Apply(horizontalCrop24bpp);
         }

         return imagesAndHistograms.Where(x => x.SliceImageResized != null).Select(x => x.SliceImageResized).ToList();
      }
   }
}