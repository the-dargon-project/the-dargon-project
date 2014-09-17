using ItzWarty.Services;
using NLog;

namespace Dargon.ModificationRepositories
{
   public class ModificationRepositoryServiceImpl : ModificationRepositoryService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      public ModificationRepositoryServiceImpl(IServiceLocator serviceLocator)
      {
         logger.Info("Initializing Modification Repository Service");
         serviceLocator.RegisterService(typeof(ModificationRepositoryService), this);
      }
   }
}
