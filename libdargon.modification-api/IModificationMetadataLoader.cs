namespace Dargon.Modifications
{
   public interface IModificationMetadataLoader
   {
      bool TryLoadMetadataFile(string metadataFilePath, out IModificationMetadata result);
   }
}