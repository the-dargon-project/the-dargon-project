namespace Dargon.Modifications
{
   public interface IModificationMetadataSerializer
   {
      bool TryLoad(string metadataFilePath, out IModificationMetadata result);
      void Save(string path, IModificationMetadata metadata);
   }
}