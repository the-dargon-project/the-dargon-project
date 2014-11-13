using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dargon.Modifications
{
   public class ModificationLoader : IModificationLoader
   {
      private readonly IModificationMetadataLoader modificationMetadataLoader;
      private readonly IBuildConfigurationLoader buildConfigurationLoader;

      public ModificationLoader(IModificationMetadataLoader modificationMetadataLoader, IBuildConfigurationLoader buildConfigurationLoader)
      {
         this.modificationMetadataLoader = modificationMetadataLoader;
         this.buildConfigurationLoader = buildConfigurationLoader;
      }

      public IModification Load(string name, string path)
      {
         return new Modification(
            modificationMetadataLoader,
            buildConfigurationLoader,
            name,
            path
         );
      }
   }
}
