namespace Dargon.Daemon
{
   public interface IDaemonConfiguration
   {
      bool IsDebugCompilation { get; }

      string UserDataDirectoryPath { get; }
      string AppDataDirectoryPath { get; }
   }
}
