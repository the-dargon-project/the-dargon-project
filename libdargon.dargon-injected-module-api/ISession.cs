namespace Dargon.InjectedModule
{
   public interface ISession
   {
      event SessionEndedEventHandler Ended;
      int ProcessId { get; }
   }
}