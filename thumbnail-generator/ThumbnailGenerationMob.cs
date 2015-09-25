using Dargon.Management;
using Dargon.Modifications.ThumbnailGenerator;

namespace Dargon.ThumbnailGenerator {
   public class ThumbnailGenerationMob {
      private readonly ThumbnailGeneratorService thumbnailGeneratorService;

      public ThumbnailGenerationMob(ThumbnailGeneratorService thumbnailGeneratorService) {
         this.thumbnailGeneratorService = thumbnailGeneratorService;
      }

      [ManagedOperation]
      public void GenerateThumbnails(string sourceDirectory, string destinationDirectory, int thumbnailsToGenerate) {
         var thumbnailGenerationParameters = new ThumbnailGenerationParameters {
            SourceDirectory = sourceDirectory,
            DestinationDirectory = destinationDirectory,
            ThumbnailsToGenerate = thumbnailsToGenerate
         };
         thumbnailGeneratorService.GenerateThumbnails(thumbnailGenerationParameters);
      }
   }
}