using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

namespace Dargon.Modifications
{
   public class BuildConfigurationLoader : IBuildConfigurationLoader
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public bool TryLoad(string path, out IBuildConfiguration result)
      {
         result = null;
         if (File.Exists(path)) {
            using (var filestream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var streamReader = new StreamReader(filestream))
            using (var jsonTextReader = new JsonTextReader(streamReader)) {
               try {
                  var serializer = new JsonSerializer();
                  result = serializer.Deserialize<BuildConfiguration>(jsonTextReader);
               } catch (Exception e) {
                  logger.Warn("Unable to load file \"" + path + "\".");
                  logger.Warn(e.ToString());
               }
            }
         }
         return result != null;
      }
   }
}
