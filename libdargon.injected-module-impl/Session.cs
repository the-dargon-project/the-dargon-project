using System;
using System.IO;
using Dargon.Transport;
using ItzWarty;
using NLog;

namespace Dargon.InjectedModule
{
   public class Session : ISession
   {
      private const string DIM_PIPE_NAME_PREFIX = "DargonInjectedModule_";
      private readonly int processId;
      private readonly BootstrapConfiguration bootstrapConfiguration;
      private readonly DtpNode node;

      public Session(int processId, BootstrapConfiguration bootstrapConfiguration, IDtpNodeFactory nodeFactory)
      {
         this.processId = processId;
         this.bootstrapConfiguration = bootstrapConfiguration;
         
         var pipeName = DIM_PIPE_NAME_PREFIX + processId;

         this.node = nodeFactory.CreateNode(true, pipeName);
      }

      public int ProcessId { get { return processId; } }
      public BootstrapConfiguration BootstrapConfiguration { get { return bootstrapConfiguration; } }

      public void HandleBootstrapComplete(IDSPExSession session)
      {

      }

      private class SessionInstructionSet : IInstructionSet
      {
         private readonly Session session;

         public SessionInstructionSet(Session session) {
            this.session = session;
         }

         public bool UseConstructionContext { get { return true; } }
         public object ConstructionContext { get { return session; } }
         public Type GetRemotelyInitializedTransactionHandlerType(byte opcode) 
         {
            if (opcode >= (byte)DTP.SYSTEM_RESERVED_BEGIN &&
                opcode <= (byte)DTP.SYSTEM_RESERVED_END) {
               switch ((DTP_DIM)opcode) {
                  case DTP_DIM.C2S_GET_BOOTSTRAP_ARGS:
                     return typeof(RITGetBootstrapArgsHandler);
                     break;
               }
            }
            return null;
         }
      }

      private class RITGetBootstrapArgsHandler : RemotelyInitializedTransactionHandler
      {
         private static readonly Logger logger = LogManager.GetCurrentClassLogger();

         private readonly Session dimSession;

         public RITGetBootstrapArgsHandler(uint transactionId, Session dimSession) 
            : base(transactionId) {
            this.dimSession = dimSession;
            }

         public override void ProcessInitialMessage(IDSPExSession session, TransactionInitialMessage message) 
         {
            logger.Info("Processing Initial Message");

            // GetBootstrapArguments initiator used to send its pid, is no longer required in dargon rewrite.
            if (message.DataLength != 4) {
               logger.Warn("Expected " + GetType().Name + " initial message to have at least 4 bytes");
            } else {
               var pid = BitConverter.ToUInt32(message.DataBuffer, message.DataOffset);
               if (pid != dimSession.ProcessId) {
                  logger.Warn("Expected " + dimSession.processId + " but got " + pid);
               }
            }

            // Send response data - properties and flags
            using (var ms = new MemoryStream()) {
               using (var writer = new BinaryWriter(ms)) {
                  var configuration = dimSession.BootstrapConfiguration;
                  var properties = configuration.Properties;
                  writer.Write((uint)properties.Count);
                  foreach (var property in properties) {
                     writer.WriteLongText(property.Key);
                     writer.WriteLongText(property.Value);
                  }

                  var flags = configuration.Flags;
                  writer.Write((uint)flags.Count);
                  foreach (var flag in flags) {
                     writer.WriteLongText(flag);
                  }
               }
               var data = ms.ToArray();
               session.SendMessage(new TransactionMessage(
                  message.TransactionId,
                  data,
                  0,
                  data.Length
               ));
            }
            session.DeregisterRITransactionHandler(this);
         }

         public override void ProcessMessage(IDSPExSession session, TransactionMessage message) 
         { 
            logger.Warn("Unexpected ProcessMessage invocation.");
         }
      }
   }
}