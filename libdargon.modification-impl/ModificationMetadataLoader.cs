using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;

namespace Dargon.Modifications
{
   public class ModificationMetadataLoader : IModificationMetadataLoader
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IFileSystemProxy fileSystemProxy;

      public ModificationMetadataLoader(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public bool TryLoadMetadataFile(string path, out IModificationMetadata result)
      {
         result = null;
         if (File.Exists(path)) {
            try {
               var json = fileSystemProxy.ReadAllText(path);
               result = JsonConvert.DeserializeObject<ModificationMetadata>(json, new GameTypeConverter());
            } catch (Exception e) {
               logger.Warn("Unable to load file \"" + path + "\".");
               logger.Warn(e.ToString());
            }
         }
         return result != null;
      }
   }
}
