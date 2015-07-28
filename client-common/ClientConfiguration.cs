using System;
using System.IO;
using System.Text;

namespace Dargon {
   public class ClientConfiguration : IClientConfiguration {
      private const string kDefaultUserDataPathName = ".dargon";
      private const string kDefaultAppDataSubdirectory = "ItzWarty/Dargon";
      private const string kDefaultConfigurationDirectoryName = "configuration";
      private const string kRepositoriesDirectoryName = "repositories";

      private readonly string defaultUserDataDirectoryPath;
      private readonly string defaultAppDataDirectoryPath;
      private readonly string defaultConfigurationDirectoryPath;
      private readonly string defaultRepositoriesDirectoryPath;

      public ClientConfiguration() {
         defaultUserDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), kDefaultUserDataPathName);
         defaultAppDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), kDefaultAppDataSubdirectory);
         defaultConfigurationDirectoryPath = Path.Combine(defaultUserDataDirectoryPath, kDefaultConfigurationDirectoryName);
         defaultRepositoriesDirectoryPath = Path.Combine(defaultUserDataDirectoryPath, kRepositoriesDirectoryName);
      }

      public string UserDataDirectoryPath { get { return defaultUserDataDirectoryPath; } }
      public string AppDataDirectoryPath { get { return defaultAppDataDirectoryPath; } }
      public string ConfigurationDirectoryPath { get { return defaultConfigurationDirectoryPath; } }
      public string RepositoriesDirectoryPath => defaultRepositoriesDirectoryPath;

      public override string ToString() {
         var sb = new StringBuilder();
         sb.AppendLine("UserDataDirectoryPath: " + UserDataDirectoryPath);
         sb.AppendLine("AppDataDirectoryPath: " + AppDataDirectoryPath);
         sb.AppendLine("ConfigurationDirectoryPath: " + ConfigurationDirectoryPath);
         sb.AppendLine("RepositoriesDirectoryPath: " + RepositoriesDirectoryPath);
         return sb.ToString();
      }
   }
}