using System;
using System.IO;
using LibGit2Sharp;
using Newtonsoft.Json;
using NLog;
using GitHubClient = Octokit.GitHubClient;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Dargon.Modifications
{
   public class Modification : IModification
   {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private const string kMetadataFileName = "metadata.json";
      private const string kBuildConfigurationFileName = "build.json";

      private readonly IModificationMetadataLoader metadataLoader;
      private readonly IBuildConfigurationLoader buildConfigurationLoader;
      private readonly string repositoryName;
      private readonly string repositoryPath;

      public Modification(IModificationMetadataLoader metadataLoader, IBuildConfigurationLoader buildConfigurationLoader, string repositoryName, string repositoryPath)
      {
         this.metadataLoader = metadataLoader;
         this.buildConfigurationLoader = buildConfigurationLoader;
         this.repositoryName = repositoryName;
         this.repositoryPath = repositoryPath;
      }

      public string RepositoryName { get { return repositoryName; } }
      public string RepositoryPath { get { return repositoryPath; } }
      public IModificationMetadata Metadata { get { return GetMetadata(); } }
      public IBuildConfiguration BuildConfiguration { get { return GetBuildConfiguration(); } }

      private IModificationMetadata GetMetadata()
      {
         var metadataFilePath = Path.Combine(repositoryPath, kMetadataFileName);

         IModificationMetadata metadata;
         if (!metadataLoader.TryLoadMetadataFile(metadataFilePath, out metadata)) {
            metadata = new ModificationMetadata();
         }
         return metadata;
      }

      private IBuildConfiguration GetBuildConfiguration() 
      { 
         var buildConfigurationFilePath = Path.Combine(repositoryPath, kBuildConfigurationFileName);

         IBuildConfiguration buildConfiguration;
         if (!buildConfigurationLoader.TryLoad(buildConfigurationFilePath, out buildConfiguration)) {
            buildConfiguration = new BuildConfiguration();
         }
         return buildConfiguration;
      }
   }
}
