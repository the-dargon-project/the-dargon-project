using System;
using System.IO;
using System.Text;

namespace Dargon {
   public class DargonConfiguration : IDargonConfiguration {
      private const string kDefaultUserDataPathName = ".dargon";
      private const string kDefaultAppDataSubdirectory = "ItzWarty/Dargon";
      private const string kDefaultConfigurationDirectoryName = "configuration";

      private readonly string defaultUserDataDirectoryPath;
      private readonly string defaultAppDataDirectoryPath;
      private readonly string defaultConfigurationDirectoryPath;

      public DargonConfiguration() {
         defaultUserDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), kDefaultUserDataPathName);
         defaultAppDataDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), kDefaultAppDataSubdirectory);
         defaultConfigurationDirectoryPath = Path.Combine(defaultUserDataDirectoryPath, kDefaultConfigurationDirectoryName);
      }

      public string UserDataDirectoryPath { get { return defaultUserDataDirectoryPath; } }
      public string AppDataDirectoryPath { get { return defaultAppDataDirectoryPath; } }
      public string ConfigurationDirectoryPath { get { return defaultConfigurationDirectoryPath; } }

      public override string ToString() {
         var sb = new StringBuilder();
         sb.AppendLine("UserDataDirectoryPath: " + UserDataDirectoryPath);
         sb.AppendLine("AppDataDirectoryPath: " + AppDataDirectoryPath);
         sb.AppendLine("ConfigurationDirectoryPath: " + ConfigurationDirectoryPath);
         return sb.ToString();
      }
   }
}