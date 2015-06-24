using System;
using Dargon.Transport;
using ItzWarty;

namespace Dargon.Trinkets.Transport.Helpers {
   public class StatelessRithFactoryImpl : StatelessRithFactory {
      private readonly Action<LimitedDSPExSession, TransactionInitialMessage> processInitialMessage;

      public StatelessRithFactoryImpl(Action<LimitedDSPExSession, TransactionInitialMessage> processInitialMessage) {
         this.processInitialMessage = processInitialMessage;
      }

      public RemotelyInitializedTransactionHandler Create(uint transactionId) {
         return new StatelessRith(transactionId, processInitialMessage);
      }

      public class StatelessRith : RemotelyInitializedTransactionHandler {
         private readonly Action<LimitedDSPExSession, TransactionInitialMessage> processInitialMessage;

         public StatelessRith(uint transactionId, Action<LimitedDSPExSession, TransactionInitialMessage> processInitialMessage) : base(transactionId) {
            this.processInitialMessage = processInitialMessage;
         }

         public override void ProcessInitialMessage(IDSPExSession session, TransactionInitialMessage message) {
            processInitialMessage(new LimitedDSPExSessionImpl(session), message);
            session.DeregisterRITransactionHandler(this);
         }

         public override void ProcessMessage(IDSPExSession session, TransactionMessage message) {
            throw new InvalidOperationException(
               "ProcessMessage invoked on stateless rith! " +
               $"TID: {message.TransactionId} " +
               $"OFF: {message.DataOffset} " +
               $"LEN: {message.DataLength} " +
               $"DAT: {message.DataBuffer.ToHex()} "
            );
         }
      }
   }
}