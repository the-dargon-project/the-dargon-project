using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Patcher;
using ItzWarty.IO;

namespace Dargon.Modifications {
   public class Modification2Factory {
      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IModificationMetadataSerializer metadataSerializer;

      public Modification2Factory(IFileSystemProxy fileSystemProxy, IModificationMetadataSerializer metadataSerializer) {
         this.fileSystemProxy = fileSystemProxy;
         this.metadataSerializer = metadataSerializer;
      }

      public Modification2 FromLocalRepository(string repositoryPath) {
         var repositoryName = fileSystemProxy.GetDirectoryInfo(repositoryPath).Name;
         var modification = new Modification2(repositoryName, repositoryPath);
         var dpmRepository = new LocalRepository(repositoryPath);
         var watcher = new Modification2MetadataWatcher(metadataSerializer, modification, dpmRepository);
         watcher.Initialize();
         return modification;
      }
   }
}
