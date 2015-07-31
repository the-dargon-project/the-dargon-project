using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using AForge.Math;
using ItzWarty;
using NLog;
using SharpDX.Direct3D9;

namespace Dargon.Modifications.ThumbnailGenerator.Impl {
   public class ThumbnailGeneratorUtilities {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly TextureConverter textureConverter;

      public ThumbnailGeneratorUtilities(TextureConverter textureConverter) {
         this.textureConverter = textureConverter;
      }

      public IReadOnlyList<T> HalfShuffle<T>(IReadOnlyList<T> input) {
         var halfCount = input.Count / 2 + (input.Count % 2);
         var half = input.Take(halfCount).ToArray();
         var remainder = input.Skip(halfCount).Take(input.Count - halfCount).Reverse().ToArray();
         var result = new T[input.Count];
         for (var i = 0; i < input.Count; i++) {
            if (i % 2 == 0) {
               result[i] = half[0];
               if (half.Length > 1) {
                  half = half.SubArray(1).Reverse().ToArray();
               }
            } else {
               result[i] = remainder[0];
               if (remainder.Length > 1) {
                  remainder = remainder.SubArray(1).Reverse().ToArray();
               }
            }
         }
         return result;
      }

      public IEnumerable<Bitmap> EnumerateBitmapsRandomly(string directory) {
         var random = new Random();
         var filePaths = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Shuffle(random);
         foreach (var filePath in filePaths) {
            Bitmap result = null;

            try {
               if (filePath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase)) {
                  result = textureConverter.ConvertToBitmap(filePath);
               } else {
                  try {
                     result = (Bitmap)Image.FromFile(filePath);
                  } catch (Exception e) {
                     switch (e.GetType().Name) {
                        case nameof(OutOfMemoryException):
                           // Invalid image format
                           break;
                        default:
                           throw;
                     }
                  }
               }
            } catch (Exception e) {
               logger.Error(e);
            }

            if (result != null) {
               yield return result;
            }
         }
      }


      public IntRange GetRangeOfWidth(Histogram histogram, int finalWidth) {
         if (finalWidth > histogram.Values.Length) {
            throw new ArgumentException();
         }

         int bias = 0;
         int left = histogram.Median;
         int right = histogram.Median;
         Func<int> width = () => right - left + 1;
         while (width() < finalWidth && left > 0 && right < histogram.Values.Length - 1) {
            if (bias == -2) {
               left--;
               bias = 0;
            } else if (bias == 2) {
               right++;
               bias = 0;
            } else if (histogram.Values[left - 1] > histogram.Values[right + 1]) {
               left--;
               bias++;
            } else {
               right++;
               bias--;
            }
         }
         if (left == 0) {
            while (width() < finalWidth) {
               right++;
            }
         } else if (right == histogram.Values.Length - 1) {
            while (width() < finalWidth) {
               left--;
            }
         }
         return new IntRange(left, right);
      }
   }
}
