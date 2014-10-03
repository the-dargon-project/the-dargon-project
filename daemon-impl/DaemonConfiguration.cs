namespace Dargon.Daemon
{
   public class DaemonConfiguration : IDaemonConfiguration
   {
      public bool IsDebugCompilation
      {
         get
         {
#if DEBUG
            return true;
#else
         return false;
#endif
         }
      }
   }
}