using Dargon.Game;
using ItzWarty.IO;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace Dargon.Modifications
{
   public class ModificationMetadataSerializer : IModificationMetadataSerializer
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IFileSystemProxy fileSystemProxy;

      public ModificationMetadataSerializer(IFileSystemProxy fileSystemProxy) {
         this.fileSystemProxy = fileSystemProxy;
      }

      public bool TryLoad(string path, out IModificationMetadata result)
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

      public void Save(string path, IModificationMetadata metadata) { 
         var json = JsonConvert.SerializeObject(metadata, Formatting.Indented,  new GameTypeConverter());
         fileSystemProxy.WriteAllText(path, json);
      }
   }
}
