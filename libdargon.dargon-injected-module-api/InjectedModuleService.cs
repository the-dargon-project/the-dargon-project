
namespace Dargon.InjectedModule
{
   public interface InjectedModuleService
   {
      ISession InjectToProcess(int processId, InjectedModuleConfiguration configuration);
   }

   public interface ISession
   {
      event SessionEndedEventHandler Ended;
      int ProcessId { get; }
   }

   public class SessionEndedEventArgs
   {
   }

   public delegate void SessionEndedEventHandler(ISession session, SessionEndedEventArgs e);
}
