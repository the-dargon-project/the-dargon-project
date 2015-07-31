using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using ItzWarty;
using Point = System.Drawing.Point;

namespace Dargon.Modifications.ThumbnailGenerator.Impl {
   public interface ThumbnailGeneratorService {
      void GenerateThumbnails(ThumbnailGenerationParameters parameters);
   }

   public class ThumbnailGeneratorServiceImpl : ThumbnailGeneratorService {
      private const int kSlicesPerThumbnail = 5;

      public void GenerateThumbnails(ThumbnailGenerationParameters parameters) {
         var textureConverterFactory = new TextureConverterFactory();
         using (var textureConverter = textureConverterFactory.Create()) {
            IFilter grayscaleFilter = new Grayscale(0.3, 0.59, 0.11);
            IInPlaceFilter edgeDetectionFilter = new HomogenityEdgeDetector();
            SliceRatingCalculator ratingCalculator = new SliceRatingCalculator();
            var utilities = new ThumbnailGeneratorUtilities(textureConverter);
            var slicePicker = new SlicePicker(grayscaleFilter, edgeDetectionFilter, ratingCalculator, utilities);
            var aspectRatio = 16 / (double)9;
            var sliceCount = 5;
            var resultWidth = 400;
            var resultHeight = (int)(resultWidth / aspectRatio);
            var sliceWidth = resultWidth / sliceCount;

            var bitmaps = utilities.EnumerateBitmapsRandomly(parameters.SourceDirectory).Take(100).ToList();

            var slices = slicePicker.PickSlices(bitmaps, new Size(sliceWidth, resultHeight));
            var sliceCombiner = new SliceCombiner(utilities, grayscaleFilter);
            for (var i = 0; i < parameters.ThumbnailsToGenerate; i++) {
               var bitmap = sliceCombiner.CombineSlices(slices, kSlicesPerThumbnail);
               using (var ms = new MemoryStream()) {
                  bitmap.Save(ms, ImageFormat.Jpeg);
                  ms.Position = 0;
                  var hash = MD5.Create().ComputeHash(ms).ToHex();
                  var outputName = hash + "_" + DateTime.UtcNow.GetUnixTime().ToString() + ".jpg";
                  var outputPath = Path.Combine(parameters.DestinationDirectory, outputName);
                  ms.Position = 0;
                  using (var fs = File.OpenWrite(outputPath)) {
                     ms.CopyTo(fs);
                  }
               }
            }
         }
      }
   }

   public class ImagesAndHistogram {
      public Bitmap OriginalImage { get; set; }
      public Bitmap EdgeImage { get; set; }
      public Histogram EdgeDensityHistogram { get; set; }
      public double Rating { get; set; }
      public Bitmap SliceImage { get; set; }
      public Bitmap SliceImageResized { get; set; }
   }

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
                  GraphicsUnit.Pixel
               );
            }
         }
         return resultImage;
      }
   }

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
