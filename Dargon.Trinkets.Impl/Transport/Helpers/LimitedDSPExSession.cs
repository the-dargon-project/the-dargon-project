using Dargon.Transport;

namespace Dargon.Trinkets.Transport.Helpers {
   public interface LimitedDSPExSession {
      void SendMessage(TransactionMessage message);
   }

   public class LimitedDSPExSessionImpl : LimitedDSPExSession {
      private readonly IDSPExSession innerSession;

      public LimitedDSPExSessionImpl(IDSPExSession innerSession) {
         this.innerSession = innerSession;
      }

      public void SendMessage(TransactionMessage message) {
         innerSession.SendMessage(message);
      }
   }
}
