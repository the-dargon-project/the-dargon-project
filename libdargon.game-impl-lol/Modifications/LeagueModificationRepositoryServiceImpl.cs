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

      public IModification GetModificationOrNull(string repositoryName) {
         var mod = modificationRepositoryService.GetModificationOrNull(repositoryName);
         if (mod == null || !mod.Metadata.Targets.Contains(GameType.LeagueOfLegends)) {
            return null;
         }
         return mod;
      }

      public void DeleteModification(IModification modification)
      {
         if (!modification.Metadata.Targets.Contains(GameType.LeagueOfLegends)) {
            throw new ArgumentException("Expected League of Legends modification");
         }
         modificationRepositoryService.DeleteModification(modification);
      }

      public IEnumerable<IModification> EnumerateModifications(GameType gameType)
      {
         if (gameType != GameType.LeagueOfLegends) {
            throw new InvalidOperationException("League Modification Repository Service only supports League of Legends modifications");
         }
         return modificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends);
      }

      public IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths) {
         return ImportLegacyModification(repositoryName, sourceRoot, sourceFilePaths, null, GameType.LeagueOfLegends);
      }

      public IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths, string friendlyName) {
         return ImportLegacyModification(repositoryName, sourceRoot, sourceFilePaths, friendlyName, GameType.LeagueOfLegends);
      }

      public IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths, GameType gameType) {
         return ImportLegacyModification(repositoryName, sourceRoot, sourceFilePaths, (string)null, gameType);
      }

      public IModification ImportLegacyModification(string repositoryName, string sourceRoot, string[] sourceFilePaths, string friendlyName, GameType gameType)
      {
         if (gameType != null && !gameType.Equals(GameType.LeagueOfLegends)) {
            throw new ArgumentException("Expected League of Legends modification");
         }
         return modificationRepositoryService.ImportLegacyModification(repositoryName, sourceRoot, sourceFilePaths, friendlyName, GameType.LeagueOfLegends);
      }

      public IEnumerable<IModification> EnumerateModifications() { return EnumerateModifications(GameType.LeagueOfLegends); }

      public void ClearModifications() 
      {
         foreach (var mod in EnumerateModifications().ToList()) {
            DeleteModification(mod);
         }
      }
   }
}