namespace Dargon.Daemon
{
   public interface IDaemonConfiguration
   {
      bool IsDebugCompilation { get; }
      string TemporaryDirectoryPath { get; }
   }
}
