namespace Dargon.InjectedModule
{
   public class SessionEndedEventArgs
   {
   }

   public delegate void SessionEndedEventHandler(ISession session, SessionEndedEventArgs e);
}