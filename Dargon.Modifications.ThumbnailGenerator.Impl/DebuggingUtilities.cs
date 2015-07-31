using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using AForge.Math;

namespace Dargon.Modifications.ThumbnailGenerator.Impl {
   public static class DebuggingUtilities {
      public static void ShowImage(Bitmap bitmap) {
         var form = new Form { ClientSize = bitmap.Size };
         var pb = new PictureBox { Image = bitmap, SizeMode = PictureBoxSizeMode.AutoSize };
         form.Controls.Add(pb);
         form.Show();
      }

      /// <summary>
      /// Must be run at end of slice selection algorithm.
      /// </summary>
      /// <param name="imagesAndHistogram"></param>
      /// <param name="utilities"></param>
      public static unsafe void ShowSliceSelectionDebugImage(ImagesAndHistogram imagesAndHistogram, ThumbnailGeneratorUtilities utilities) {
         var edgeImage = imagesAndHistogram.EdgeImage;
         var edgeDensityHistogram = imagesAndHistogram.EdgeDensityHistogram;

         DrawHistogram(edgeImage, edgeDensityHistogram);
         DrawVerticalLine(edgeImage, edgeDensityHistogram.Median, 254);
         DrawVerticalLine(edgeImage, (int)edgeDensityHistogram.Mean, 253);
         DrawVerticalLine(edgeImage, (int)(edgeDensityHistogram.Median - edgeDensityHistogram.StdDev), 252);
         DrawVerticalLine(edgeImage, (int)(edgeDensityHistogram.Median + edgeDensityHistogram.StdDev), 252);

         var resultAspect = 16.0 / 10.0;
         var sliceCount = 4;
         var sliceAspect = resultAspect / sliceCount;
         var desiredWidth = (int)(edgeImage.Height * sliceAspect);

         var range = utilities.GetRangeOfWidth(edgeDensityHistogram, desiredWidth);
         DrawVerticalLine(edgeImage, range.Min, 250);
         DrawVerticalLine(edgeImage, range.Max, 250);
         ShowImage(edgeImage);
         Application.Run();
      }


      public static unsafe void DrawHistogram(Bitmap edgeDetectedBitmap, Histogram histogram) {
         var pallete = edgeDetectedBitmap.Palette;
         var palleteEntries = pallete.Entries;
         palleteEntries[250] = Color.Magenta;
         palleteEntries[251] = Color.Yellow;
         palleteEntries[252] = Color.Cyan;
         palleteEntries[253] = Color.Lime;
         palleteEntries[254] = Color.Red;
         palleteEntries[255] = Color.White;
         edgeDetectedBitmap.Palette = pallete;

         var histogramHeight = Math.Min(100, edgeDetectedBitmap.Height);
         var rect = new Rectangle(Point.Empty, edgeDetectedBitmap.Size);
         var bitmapData = edgeDetectedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
         var histogramMax = histogram.Values.Max();
         for (var x = 0; x < edgeDetectedBitmap.Width; x++) {
            var barHeight = histogram.Values[x] * histogramHeight / histogramMax;
            Console.WriteLine(barHeight);
            var pCurrentPixel = (byte*)bitmapData.Scan0.ToPointer() + x;
            for (var y = 0; y < barHeight; y++) {
               *pCurrentPixel = 255;
               pCurrentPixel += bitmapData.Stride;
            }
         }
         edgeDetectedBitmap.UnlockBits(bitmapData);
      }

      public static unsafe void DrawVerticalLine(Bitmap edgeDetectedBitmap, int x, byte value) {
         var rect = new Rectangle(Point.Empty, edgeDetectedBitmap.Size);
         var bitmapData = edgeDetectedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

         var pCurrentPixel = (byte*)bitmapData.Scan0.ToPointer() + x;
         for (var y = 0; y < edgeDetectedBitmap.Height; y++) {
            *pCurrentPixel = value;
            pCurrentPixel += bitmapData.Stride;
         }
         edgeDetectedBitmap.UnlockBits(bitmapData);
      }
   }
}
