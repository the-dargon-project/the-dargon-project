using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.IO;
using NLog;

namespace Dargon.Modifications {
   public class ModificationComponentFactory {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IFileSystemProxy fileSystemProxy;
      private readonly IPofContext pofContext;
      private readonly SlotSourceFactory slotSourceFactory;
      private readonly IPofSerializer serializer;

      public ModificationComponentFactory(IFileSystemProxy fileSystemProxy, IPofContext pofContext, SlotSourceFactory slotSourceFactory, IPofSerializer serializer) {
         this.fileSystemProxy = fileSystemProxy;
         this.pofContext = pofContext;
         this.slotSourceFactory = slotSourceFactory;
         this.serializer = serializer;
      }

      public TComponent Create<TComponent>(Modification modification, string metadataRootPath) where TComponent : Component {
         fileSystemProxy.PrepareDirectory(metadataRootPath);
         File.SetAttributes(metadataRootPath, File.GetAttributes(metadataRootPath) | FileAttributes.Hidden);

         var componentAttribute = typeof(TComponent).GetAttributeOrNull<ModificationComponentAttribute>();
         var originSubdirectoryName = componentAttribute.Origin.GetAttributeOrNull<SubdirectoryAttribute>().Name;
         var path = Path.Combine(metadataRootPath, originSubdirectoryName, componentAttribute.FileName);
         fileSystemProxy.PrepareParentDirectory(path);

         var constructors = typeof(TComponent).GetConstructors();
         var modificationParameterConstructor = constructors.Where(x => x.GetParameters().Length == 1).FirstOrDefault(x => x.GetParameters().First().ParameterType == typeof(Modification));
         TComponent component;
         if (modificationParameterConstructor != null) {
            Console.WriteLine("Calling mod constructor on " + typeof(TComponent));
            component = (TComponent)modificationParameterConstructor.Invoke(new object[] { modification });
         } else {
            var parameterlessConstructor = typeof(TComponent).GetConstructors().First(x => x.GetParameters().None());
            component = (TComponent)parameterlessConstructor.Invoke(null);
         }

         if (fileSystemProxy.Exists(path)) {
            logger.Info("Path already exists! " + path);
            using (var fs = fileSystemProxy.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
               var newComponent = serializer.Deserialize<TComponent>(fs.Reader);
               component.Load(newComponent);
            }
         }

         object synchronization = new object();
         var fsw = new FileSystemWatcher(new DirectoryInfo(modification.RepositoryPath).Parent.FullName);
         fsw.EnableRaisingEvents = true;
         fsw.Changed += (s, e) => {
            if (Path.GetFullPath(e.FullPath) != Path.GetFullPath(path)) {
               return;
            }
            if (e.ChangeType == WatcherChangeTypes.Deleted) {
               fsw.Dispose();
            } else {
               lock (synchronization) {
                  using (var fs = fileSystemProxy.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                     var newComponent = serializer.Deserialize<TComponent>(fs.Reader);
                     component.Load(newComponent);
                  }
               }
            }
         };
         component.PropertyChanged += (s, e) => {
            fileSystemProxy.PrepareParentDirectory(path);
            logger.Info("BEGIN WRITING FILE " + path);
            using (var fs = fileSystemProxy.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
               serializer.Serialize(fs.Writer, component);
            }
            logger.Info("END WRITING FILE " + path);
         };
         return component;
      }
   }
}