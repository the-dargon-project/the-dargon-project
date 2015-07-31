using System.Diagnostics;
using Dargon.PortableObjects;

namespace Dargon.Modifications.ThumbnailGenerator {
   public class ThumbnailGenerationParameters : IPortableObject {
      private const uint kVersion = 0;

      public string SourceDirectory { get; set; }
      public string DestinationDirectory { get; set; }
      public int ThumbnailsToGenerate { get; set; }

      public void Serialize(IPofWriter writer) {
         writer.WriteU32(0, kVersion);
         writer.WriteString(1, SourceDirectory);
         writer.WriteString(2, DestinationDirectory);
         writer.WriteS32(3, ThumbnailsToGenerate);
      }

      public void Deserialize(IPofReader reader) {
         var version = reader.ReadU32(0);
         SourceDirectory = reader.ReadString(1);
         DestinationDirectory = reader.ReadString(2);
         ThumbnailsToGenerate = reader.ReadS32(3);

         Trace.Assert(version == kVersion, "version == kVersion");
      }
   }
}
