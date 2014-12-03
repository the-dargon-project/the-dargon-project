namespace Dargon {
   public interface IDargonConfiguration {
      string UserDataDirectoryPath { get; }
      string AppDataDirectoryPath { get; }
      string ConfigurationDirectoryPath { get; }
   }
}
