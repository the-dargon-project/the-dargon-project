namespace Dargon {
   public interface IClientConfiguration {
      string UserDataDirectoryPath { get; }
      string AppDataDirectoryPath { get; }
      string ConfigurationDirectoryPath { get; }
      string RepositoriesDirectoryPath { get; }
   }
}
