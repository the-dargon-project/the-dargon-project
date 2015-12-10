using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AForge.Imaging.Filters;
using Dargon.DDS;
using ItzWarty;

namespace Dargon.Modifications.ThumbnailGenerator {
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
}
