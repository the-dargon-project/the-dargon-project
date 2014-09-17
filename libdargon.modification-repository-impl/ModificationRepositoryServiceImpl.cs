using ItzWarty.Services;

namespace Dargon.ModificationRepositories
{
   public class ModificationRepositoryServiceImpl : ModificationRepositoryService
   {
      public ModificationRepositoryServiceImpl(IServiceLocator serviceLocator)
      {
         serviceLocator.RegisterService(typeof(ModificationRepositoryService), this);
      }
   }
}
