using System;
using System.Collections.Generic;
using System.Linq;
using Dargon.Game;
using Dargon.ModificationRepositories;
using Dargon.Modifications;

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

      public void AddModification(IModification modification) {
         if (!modification.Metadata.Targets.Contains(GameType.LeagueOfLegends)) {
            throw new ArgumentException("Expected League of Legends modification");
         }
         modificationRepositoryService.AddModification(modification);
      }

      public void RemoveModification(IModification modification)
      {
         if (!modification.Metadata.Targets.Contains(GameType.LeagueOfLegends)) {
            throw new ArgumentException("Expected League of Legends modification");
         }
         modificationRepositoryService.RemoveModification(modification);
      }

      public IEnumerable<IModification> EnumerateModifications(GameType gameType)
      {
         if (gameType != GameType.LeagueOfLegends) {
            throw new InvalidOperationException("League Modification Repository Service only supports League of Legends modifications");
         }
         return modificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends);
      }

      public IEnumerable<IModification> EnumerateModifications() { return EnumerateModifications(GameType.LeagueOfLegends); }
   }
}