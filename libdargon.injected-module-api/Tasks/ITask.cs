namespace Dargon.InjectedModule.Tasks
{
   public interface ITask
   {
      string Type { get; }
      byte[] Data { get; }
   }
}