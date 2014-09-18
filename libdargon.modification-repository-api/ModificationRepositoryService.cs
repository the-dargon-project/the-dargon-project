
namespace Dargon.ModificationRepositories
{
   public interface ModificationRepositoryService
   {
      void ClearModifications();
      void ImportLegacyModification(string root, string[] filePaths);
   }
}
