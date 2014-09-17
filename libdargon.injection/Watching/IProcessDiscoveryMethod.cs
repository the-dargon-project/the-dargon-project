namespace Dargon.Processes.Watching
{
   public interface IProcessDiscoveryMethod
   {
      event OnProcessDiscovered ProcessDiscovered;
      void Start();
      void Stop();
   }
}
