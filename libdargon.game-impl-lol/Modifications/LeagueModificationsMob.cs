using Dargon.Management;
using Dargon.Modifications;
using ItzWarty;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dargon.LeagueOfLegends.Modifications {
   public class LeagueModificationsMob {
      private readonly ClientConfiguration clientConfiguration;
      private readonly ModificationLoader modificationLoader;
      private readonly LeagueBuildUtilities leagueBuildUtilities;

      public LeagueModificationsMob(ClientConfiguration clientConfiguration, ModificationLoader modificationLoader, LeagueBuildUtilities leagueBuildUtilities) {
         this.clientConfiguration = clientConfiguration;
         this.modificationLoader = modificationLoader;
         this.leagueBuildUtilities = leagueBuildUtilities;
      }

      [ManagedOperation]
      public string EnumerateModifications() {
         return modificationLoader.EnumerateModifications().Select(mod => mod.RepositoryName + " " + mod.RepositoryPath).Join("\r\n");
      }

      [ManagedOperation]
      public void ResolveModification(string name) {
         var repositoryPath = Path.Combine(Path.Combine(clientConfiguration.UserDataDirectoryPath, "repositories"), name);
         var modification = modificationLoader.FromPath(repositoryPath);
         leagueBuildUtilities.ResolveModification(modification, CancellationToken.None);
      }

      [ManagedOperation]
      public void CompileModification(string name) {
         var repositoryPath = Path.Combine(Path.Combine(clientConfiguration.UserDataDirectoryPath, "repositories"), name);
         var modification = modificationLoader.FromPath(repositoryPath);
         leagueBuildUtilities.CompileModification(modification, CancellationToken.None);
      }

      [ManagedOperation]
      public string LinkAirModifications() {
         var repositoriesPath = Path.Combine(clientConfiguration.UserDataDirectoryPath, "repositories");
         var repositoryPaths = Directory.GetDirectories(repositoriesPath);
         var modifications = repositoryPaths.Select(modificationLoader.FromPath).ToArray();

         var stopwatch = new Stopwatch();
         stopwatch.Start();
         var changes = leagueBuildUtilities.LinkAirModifications(modifications);
         var sb = new StringBuilder();
         sb.AppendLine("Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds));
         foreach (var change in changes) {
            var data = change.Data;
            sb.AppendLine(change.Type + ": " + Util.Generate(data.Length, i => data[i] >= 32 && data[i] <= 126 ? (char)data[i] : '?').Join(""));
         }
         return sb.ToString();
      }

      [ManagedOperation]
      public string LinkGameModifications() {
         var repositoriesPath = Path.Combine(clientConfiguration.UserDataDirectoryPath, "repositories");
         var repositoryPaths = Directory.GetDirectories(repositoriesPath);
         var modifications = repositoryPaths.Select(modificationLoader.FromPath).ToArray();

         var stopwatch = new Stopwatch();
         stopwatch.Start();
         var changes = leagueBuildUtilities.LinkGameModifications(modifications);
         var sb = new StringBuilder();
         sb.AppendLine("Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds));
         foreach (var change in changes) {
            var data = change.Data;
            sb.AppendLine(change.Type + ": " + Util.Generate(data.Length, i => data[i] >= 32 && data[i] <= 126 ? (char)data[i] : '?').Join(""));
         }
         return sb.ToString();
      }

      //      private readonly LeagueModificationRepositoryService leagueModificationRepositoryService;
      //      private readonly LeagueModificationResolutionService leagueModificationResolutionService;
      //      private readonly LeagueModificationObjectCompilerService leagueModificationObjectCompilerService;
      //      private readonly LeagueGameModificationLinkerService leagueGameModificationLinkerService;
      //
      //      public LeagueModificationsMob(LeagueModificationRepositoryService leagueModificationRepositoryService, LeagueModificationResolutionService leagueModificationResolutionService, LeagueModificationObjectCompilerService leagueModificationObjectCompilerService, LeagueGameModificationLinkerService leagueGameModificationLinkerService) {
      //         this.leagueModificationRepositoryService = leagueModificationRepositoryService;
      //         this.leagueModificationResolutionService = leagueModificationResolutionService;
      //         this.leagueModificationObjectCompilerService = leagueModificationObjectCompilerService;
      //         this.leagueGameModificationLinkerService = leagueGameModificationLinkerService;
      //      }
      //
      //      [ManagedOperation]
      //      public string ImportModification(string modificationName, string sourcePath) {
      //         try {
      //            if (!Directory.Exists(sourcePath)) {
      //               return "Error! The specified path either does not exist or is not a directory!";
      //            } else {
      //               sourcePath = Path.GetFullPath(sourcePath);
      //               var mod = leagueModificationRepositoryService.ImportLegacyModification(
      //                  modificationName,
      //                  sourcePath,
      //                  Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories),
      //                  modificationName.ToTitleCase()
      //               );
      //               return $"Imported mod {mod.RepositoryName} to {mod.RepositoryPath}!";
      //            }
      //         } catch (Exception e) {
      //            return "Error! " + e;
      //         }
      //      }
      //      [ManagedOperation]
      //      public string DeleteModification(string modificationName) {
      //         try {
      //            leagueModificationRepositoryService.DeleteModification(leagueModificationRepositoryService.GetModificationOrNull(modificationName));
      //            return "Success!";
      //         } catch (Exception e) {
      //            return "Error! " + e;
      //         }
      //      }
      //      [ManagedOperation]
      //      public string EnumerateModifications() {
      //         var sb = new StringBuilder();
      //         var mods = leagueModificationRepositoryService.EnumerateModifications(GameType.LeagueOfLegends);
      //         foreach (var mod in mods) {
      //            sb.AppendLine(mod.RepositoryName + " " + mod.Metadata.Name + " " + mod.RepositoryPath);
      //         }
      //         return sb.ToString();
      //      }
      //
      //      [ManagedOperation]
      //      public string ResolveModification(string repositoryName) {
      //         return ResolveModificationInternal(repositoryName, ModificationTargetType.Client | ModificationTargetType.Game);
      //      }
      //
      //      [ManagedOperation]
      //      public string ResolveModificationClient(string repositoryName) {
      //         return ResolveModificationInternal(repositoryName, ModificationTargetType.Client);
      //      }
      //
      //      [ManagedOperation]
      //      public string ResolveModificationGame(string repositoryName) {
      //         return ResolveModificationInternal(repositoryName, ModificationTargetType.Game);
      //      }
      //
      //      private string ResolveModificationInternal(string repositoryName, ModificationTargetType modificationTargetType) {
      //         var mod = leagueModificationRepositoryService.GetModificationOrNull(repositoryName);
      //         if (mod == null) {
      //            return "Unable to find mod of name " + repositoryName;
      //         }
      //
      //         var stopwatch = new Stopwatch();
      //         stopwatch.Start();
      //         var resolution = leagueModificationResolutionService.StartModificationResolution(mod, modificationTargetType);
      //         resolution.WaitForChainCompletion();
      //         return "Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds);
      //      }
      //
      //      [ManagedOperation]
      //      public string CompileModificationObjects(string repositoryName) {
      //         var mod = leagueModificationRepositoryService.GetModificationOrNull(repositoryName);
      //         if (mod == null) {
      //            return "Unable to find mod of name " + repositoryName;
      //         }
      //
      //         var stopwatch = new Stopwatch();
      //         stopwatch.Start();
      //         var task = leagueModificationObjectCompilerService.CompileObjects(mod, ModificationTargetType.Client | ModificationTargetType.Game);
      //         task.WaitForChainCompletion();
      //         return "Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds);
      //      }
      //
      //      [ManagedOperation]
      //      public string LinkModifications() {
      //         var stopwatch = new Stopwatch();
      //         stopwatch.Start();
      //         var changes = leagueGameModificationLinkerService.LinkModificationObjects();
      //         var sb = new StringBuilder();
      //         sb.AppendLine("Completed in {0} milliseconds".F(stopwatch.ElapsedMilliseconds));
      //         foreach (var change in changes) {
      //            var data = change.Data;
      //            sb.AppendLine(change.Type + ": " + Util.Generate(data.Length, i => data[i] >= 32 && data[i] <= 126 ? (char)data[i] : '?').Join(""));
      //         }
      //         return sb.ToString();
      //      }
   }
}
