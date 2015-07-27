using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Modifications
{
   public class ModificationLoader : IModificationLoader
   {
      private readonly IModificationMetadataSerializer modificationMetadataSerializer;
      private readonly IBuildConfigurationLoader buildConfigurationLoader;

      public ModificationLoader(IModificationMetadataSerializer modificationMetadataSerializer, IBuildConfigurationLoader buildConfigurationLoader)
      {
         this.modificationMetadataSerializer = modificationMetadataSerializer;
         this.buildConfigurationLoader = buildConfigurationLoader;
      }

      public IModification Load(string name, string path)
      {
         return new ModificationOld(
            modificationMetadataSerializer,
            buildConfigurationLoader,
            name,
            path
         );
      }
   }
}
