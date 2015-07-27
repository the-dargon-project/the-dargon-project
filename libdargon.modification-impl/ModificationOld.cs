using Dargon.PortableObjects;
using NLog;
using System.IO;

namespace Dargon.Modifications
{
   public class ModificationOld : IModification, IPortableObject
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private Data data;
      private string repositoryName;
      private string repositoryPath;

      public ModificationOld() {
         data = new DataPofImpl();
      }

      public ModificationOld(IModificationMetadataSerializer metadataSerializer, IBuildConfigurationLoader buildConfigurationLoader, string repositoryName, string repositoryPath) {
         this.data = new DataLazyImpl(this, metadataSerializer, buildConfigurationLoader);
         this.repositoryName = repositoryName;
         this.repositoryPath = repositoryPath;
      }

      public string RepositoryName { get { return repositoryName; } }
      public string RepositoryPath { get { return repositoryPath; } }
      public IModificationMetadata Metadata { get { return data.Metadata; } }
      public IBuildConfiguration BuildConfiguration { get { return data.BuildConfiguration; } }
      public bool IsEnabled => !File.Exists(Path.Combine(repositoryPath, ".dpm/metadata/DISABLED"));

      public void Serialize(IPofWriter writer) {
         writer.WriteString(0, repositoryName);
         writer.WriteString(1, repositoryPath);
         writer.WriteObject(2, data.Metadata);
         writer.WriteObject(3, data.BuildConfiguration);
      }

      public void Deserialize(IPofReader reader) {
         repositoryName = reader.ReadString(0);
         repositoryPath = reader.ReadString(1);
         var data = new DataPofImpl();
         data.Metadata = reader.ReadObject<ModificationMetadata>(2);
         data.BuildConfiguration = reader.ReadObject<BuildConfiguration>(3);
         this.data = data;
      }

      private interface Data {
         IModificationMetadata Metadata { get; }
         IBuildConfiguration BuildConfiguration { get; }
      }

      private class DataLazyImpl : Data {
         private readonly ModificationOld modification;
         private readonly IModificationMetadataSerializer metadataSerializer;
         private readonly IBuildConfigurationLoader buildConfigurationLoader;

         public DataLazyImpl(ModificationOld modification, IModificationMetadataSerializer metadataSerializer, IBuildConfigurationLoader buildConfigurationLoader) {
            this.modification = modification;
            this.metadataSerializer = metadataSerializer;
            this.buildConfigurationLoader = buildConfigurationLoader;
         }

         public IModificationMetadata Metadata { get { return GetMetadata(); } }
         public IBuildConfiguration BuildConfiguration { get { return GetBuildConfiguration(); } }

         private IModificationMetadata GetMetadata() {
            var metadataFilePath = Path.Combine(modification.RepositoryPath, ModificationConstants.kMetadataFileName);

            IModificationMetadata metadata;
            if (!metadataSerializer.TryLoad(metadataFilePath, out metadata)) {
               metadata = new ModificationMetadata();
            }
            return metadata;
         }

         private IBuildConfiguration GetBuildConfiguration() {
            var buildConfigurationFilePath = Path.Combine(modification.repositoryPath, ModificationConstants.kBuildConfigurationFileName);

            IBuildConfiguration buildConfiguration;
            if (!buildConfigurationLoader.TryLoad(buildConfigurationFilePath, out buildConfiguration)) {
               buildConfiguration = new BuildConfiguration();
            }
            return buildConfiguration;
         }
      }

      public class DataPofImpl : Data {
         public IModificationMetadata Metadata { get; set; }
         public IBuildConfiguration BuildConfiguration { get; set; }
      }
   }
}
