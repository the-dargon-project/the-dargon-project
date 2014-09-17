namespace Dargon.Daemon
{
   class Program
   {
      public static void Main(string[] args) { 
         var core = new DaemonServiceImpl();
         core.Run();
      }
   }
}
