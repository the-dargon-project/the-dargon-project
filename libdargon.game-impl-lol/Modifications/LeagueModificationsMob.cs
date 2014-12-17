using System.Diagnostics;
using Dargon.Management;
using System.Text;
using Dargon.Game;
using ItzWarty;

namespace Dargon.LeagueOfLegends.Modifications {
   public class LeagueModificationsMob {
      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService;
      private readonly LeagueGameModificationLinkerService leagueGameModificationLinkerService;

      public LeagueModificationsMob(LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueModificationResolutionService leagueModificationResolutionService, LeagueModificationObjectCompilerService leagueModificationObjectCompilerService, LeagueGameModificationLinkerService leagueGameModificationLinkerService) {
         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
         this.leagueModificationResolutionService = leagueModificationResolutionService;
         this.leagueModificationObjectCompilerService = leagueModificationObjectCompilerService;
         this.leagueGameModificationLinkerService = leagueGameModificationLinkerService;
      }

      [ManagedOperation]
      public string EnumerateModifications() {
         var sb = new StringBuilder();
         var mods = leagueModificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends);
         foreach (var mod in mods) {
            sb.AppendLine(mod.RepositoryName + " " + mod.Metadata.Name + " " + mod.RepositoryPath);
         }
         return sb.ToString();
      }

      [ManagedOperation]
      public string ResolveModification(string repositoryName) {
         return ResolveModificationInternal(repositoryName, ModificationTargetType.Client | ModificationTargetType.Game);
      }

      [ManagedOperation]
      public string ResolveModificationClient(string repositoryName) {
         return ResolveModificationInternal(repositoryName, ModificationTargetType.Client);
      }

      [ManagedOperation]
      public string ResolveModificationGame(string repositoryName) {
         return ResolveModificationInternal(repositoryName, ModificationTargetType.Game);
      }

      private string ResolveModificationInternal(string repositoryName, ModificationTargetType modificationTargetType) {
         var mod = leagueModificationRepositoryService.GetModificationOrNull(repositoryName);
         if (mod == null) {
            return "Unable to find mod of name " + repositoryName;
         }

         var stopwatch = new Stopwatch();
         stopwatch.Start();
         var resolution = leagueModificationResolutionService.StartModificationResolution(mod, modificationTargetType);
         resolution.WaitForChainCompletion();
         return "Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds);
      }

      [ManagedOperation]
      public string CompileModificationObjects(string repositoryName) {
         var mod = leagueModificationRepositoryService.GetModificationOrNull(repositoryName);
         if (mod == null) {
            return "Unable to find mod of name " + repositoryName;
         }

         var stopwatch = new Stopwatch();
         stopwatch.Start();
         var task = leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client | ModificationTargetType.Game);
         task.WaitForChainCompletion();
         return "Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds);
      }

      [ManagedOperation]
      public string LinkModifications() {
         var stopwatch = new Stopwatch();
         stopwatch.Start();
         leagueGameModificationLinkerService.LinkModificationObjects();
         return "Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds);
      }
   }
}
