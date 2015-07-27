using System.IO;
using Dargon.PortableObjects;
using ItzWarty;
using ItzWarty.IO;

namespace Dargon.Modifications {
   public class ModificationComponentFactory {
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

      public TComponent Create<TComponent>(string metadataRootPath) where TComponent : Component, new() {
         fileSystemProxy.PrepareDirectory(metadataRootPath);

         var componentAttribute = typeof(TComponent).GetAttributeOrNull<ModificationComponentAttribute>();
         var originSubdirectoryName = componentAttribute.Origin.GetAttributeOrNull<SubdirectoryAttribute>().Name;
         var path = Path.Combine(metadataRootPath, originSubdirectoryName, componentAttribute.FileName);

         TComponent component;
         try {
            using (var fs = fileSystemProxy.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
               component = serializer.Deserialize<TComponent>(fs.Reader);
            }
         } catch {
            component = new TComponent();
         }

         component.PropertyChanged += (s, e) => {
            fileSystemProxy.PrepareParentDirectory(path);
            using (var fs = fileSystemProxy.OpenFile(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
               serializer.Serialize(fs.Writer, component);
            }
         };
         var fsw = new FileSystemWatcher(metadataRootPath);
         fsw.Changed += (s, e) => {
            using (var fs = fileSystemProxy.OpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
               var slotSource = slotSourceFactory.CreateFromBinaryReader(fs.Reader);
               component.Deserialize(new PofReader(pofContext, slotSource));
            }
         };
         return component;
      }
   }
}