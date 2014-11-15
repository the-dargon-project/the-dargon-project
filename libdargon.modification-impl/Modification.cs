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

      private readonly IModificationMetadataSerializer metadataSerializer;
      private readonly IBuildConfigurationLoader buildConfigurationLoader;
      private readonly string repositoryName;
      private readonly string repositoryPath;

      public Modification(IModificationMetadataSerializer metadataSerializer, IBuildConfigurationLoader buildConfigurationLoader, string repositoryName, string repositoryPath)
      {
         this.metadataSerializer = metadataSerializer;
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
         var metadataFilePath = Path.Combine(repositoryPath, ModificationConstants.kMetadataFileName);

         IModificationMetadata metadata;
         if (!metadataSerializer.TryLoad(metadataFilePath, out metadata)) {
            metadata = new ModificationMetadata();
         }
         return metadata;
      }

      private IBuildConfiguration GetBuildConfiguration() 
      {
         var buildConfigurationFilePath = Path.Combine(repositoryPath, ModificationConstants.kBuildConfigurationFileName);

         IBuildConfiguration buildConfiguration;
         if (!buildConfigurationLoader.TryLoad(buildConfigurationFilePath, out buildConfiguration)) {
            buildConfiguration = new BuildConfiguration();
         }
         return buildConfiguration;
      }
   }
}
