using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Patcher;

namespace Dargon.Modifications {
   public class Modification2MetadataWatcher {
      private const string kMetadataJsonFileName = "metadata.json";
      private readonly IModificationMetadataSerializer metadataSerializer;
      private readonly Modification2 modification;
      private readonly LocalRepository dpmRepository;
      private IModificationMetadata metadata;

      public Modification2MetadataWatcher(IModificationMetadataSerializer metadataSerializer, Modification2 modification, LocalRepository dpmRepository) {
         this.metadataSerializer = metadataSerializer;
         this.modification = modification;
         this.dpmRepository = dpmRepository;
      }

      public string MetadataFilePath => Path.Combine(modification.RepositoryPath, kMetadataJsonFileName);
      public string DisabledFilePath => dpmRepository.GetMetadataFilePath("DISABLED");

      public void Initialize() {
         if (!metadataSerializer.TryLoad(MetadataFilePath, out metadata)) {
            throw new NotImplementedException();
         } else {
            modification.FriendlyName = metadata.Name;
            modification.Authors = metadata.Authors;
         }
         modification.IsEnabled = !File.Exists(DisabledFilePath);
         modification.PropertyChanged += HandleModificationPropertyChanged;
      }

      private void HandleModificationPropertyChanged(object sender, PropertyChangedEventArgs e) {
         bool requireJsonSave = false;
         switch (e.PropertyName) {
            case nameof(modification.FriendlyName):
               metadata.Name = MetadataFilePath;
               requireJsonSave = true;
               break;
            case nameof(modification.IsEnabled):
               if (modification.IsEnabled) {
                  File.Delete(DisabledFilePath);
               } else {
                  File.Open(DisabledFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite).Close();
               }
               break;
         }
         if (requireJsonSave) {
            metadataSerializer.Save(MetadataFilePath, metadata);
         }
      }
   }
}
