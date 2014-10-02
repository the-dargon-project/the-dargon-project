using Dargon.Game;
using Dargon.Modifications;
using ItzWarty.Collections;
using ItzWarty.Services;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace Dargon.ModificationRepositories
{
   public class ModificationRepositoryServiceImpl : ModificationRepositoryService
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();
      private readonly ConcurrentSet<IModification> modifications = new ConcurrentSet<IModification>(); 

      public ModificationRepositoryServiceImpl(IServiceLocator serviceLocator)
      {
         logger.Info("Initializing Modification Repository Service");

         serviceLocator.RegisterService(typeof(ModificationRepositoryService), this);
      }

      public void ClearModifications() 
      {
         logger.Info("Clearing All Modifications");
         modifications.Clear(); 
      }

      public void AddModification(IModification modification)
      {
         logger.Info("Adding Modification " + modification); 
         modifications.TryAdd(modification); 
      }

      public void RemoveModification(IModification modification)
      {
         logger.Info("Removing Modification " + modification);
         modifications.TryRemove(modification);
      }

      public IEnumerable<IModification> EnumerateModifications(GameType gameType)
      {
         if (gameType == GameType.Any) 
            return modifications;
         else
            return modifications.Where(mod => mod.GameType == gameType);
      }
   }
}
