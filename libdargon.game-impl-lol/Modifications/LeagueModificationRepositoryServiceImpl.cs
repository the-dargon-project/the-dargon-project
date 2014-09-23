using System;
using System.Collections.Generic;
using System.Linq;
using Dargon.Game;
using Dargon.ModificationRepositories;

namespace Dargon.LeagueOfLegends.Modifications
{
   class LeagueModificationRepositoryServiceImpl : LeagueModificationRepositoryService
   {
      public readonly ModificationRepositoryService modificationRepositoryService;
      
      public LeagueModificationRepositoryServiceImpl(ModificationRepositoryService modificationRepositoryService) 
      { 
         this.modificationRepositoryService = modificationRepositoryService; 
      }

      public void ClearModifications() 
      {
         foreach (var mod in EnumerateModifications().ToList()) {
            RemoveModification(mod);
         }
      }

      public void AddModification(ILeagueModification modification) {
         if (modification.GameType != GameType.LeagueOfLegends) {
            throw new ArgumentException("Expected League of Legends modification");
         }
         modificationRepositoryService.AddModification(modification);
      }

      public void RemoveModification(ILeagueModification modification) { modificationRepositoryService.RemoveModification(modification); }
      
      public IEnumerable<ILeagueModification> EnumerateModifications() { return modificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends).Cast<ILeagueModification>(); }
   }
}