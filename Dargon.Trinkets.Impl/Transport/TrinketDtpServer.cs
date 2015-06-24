using Dargon.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using Dargon.Trinkets.Components;
using Dargon.Trinkets.Transport.Helpers;
using ItzWarty;
using ItzWarty.IO;
using NLog;

namespace Dargon.Trinkets.Transport {
   public interface TrinketDtpServer {
      /// <summary>
      /// Signals the end of a trinket dtp session. As trinket expects only
      /// one connection per dtp endpoint, this also indicates that the
      /// trinket dtp wrapper may be disposed.
      /// </summary>
      event EventHandler Completed;
   }

   public class TrinketDtpServerImpl : TrinketDtpServer {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly IStreamFactory streamFactory;
      private readonly BootstrapConfigurationGenerator bootstrapConfigurationGenerator;
      private readonly IDtpNode transportNode;
      private readonly TrinketStartupConfiguration startupConfiguration;

      public event EventHandler Completed;

      public TrinketDtpServerImpl(IStreamFactory streamFactory, BootstrapConfigurationGenerator bootstrapConfigurationGenerator, IDtpNode transportNode, TrinketStartupConfiguration startupConfiguration) {
         this.streamFactory = streamFactory;
         this.bootstrapConfigurationGenerator = bootstrapConfigurationGenerator;
         this.transportNode = transportNode;
         this.startupConfiguration = startupConfiguration;
      }

      public void Preinitialize(IDictionary<byte, Func<uint, RemotelyInitializedTransactionHandler>> rithFactoriesByOpcodes) {
         rithFactoriesByOpcodes.Add((byte)DTP_DIM.C2S_GET_BOOTSTRAP_ARGS, new StatelessRithFactoryImpl(HandleGetBootstrapArguments).Create);
         rithFactoriesByOpcodes.Add((byte)DTP_DIM.C2S_GET_INITIAL_COMMAND_LIST, new StatelessRithFactoryImpl(HandleGetInitialCommandList).Create);
         rithFactoriesByOpcodes.Add((byte)DTP_DIM.C2S_REMOTE_LOG, new StatelessRithFactoryImpl(HandleRemoteLog).Create);
      }

      public void Initialize() {
         transportNode.ClientConnected += HandleClientConnected;
      }

      public void HandleClientConnected(DtpNode sender, ClientConnectedEventArgs e) {
         e.Session.Disconnected += HandleClientDisconnected;
         transportNode.ClientConnected -= HandleClientConnected;
      }

      private void HandleClientDisconnected(DtpNodeSession sender, ClientDisconnectedEventArgs e) {
         transportNode.Shutdown();
         sender.Disconnected -= HandleClientDisconnected;
         Completed?.Invoke(this, EventArgs.Empty);
      }

      private void HandleGetBootstrapArguments(LimitedDSPExSession session, TransactionInitialMessage message) {
         if (message.DataLength != 4) {
            logger.Warn($"Expected {GetType().Name} initial message to have at least 4 bytes");
         } else {
            // In the future, we should validate that the PID is the expected PID.
            var pid = BitConverter.ToUInt32(message.DataBuffer, message.DataOffset);
            logger.Info($"GetBootstrapArguments: sender pid is {pid}");
         }

         // Send response data - properties and flags
         using (var ms = streamFactory.CreateMemoryStream()) {
            var writer = ms.Writer;

            var configuration = bootstrapConfigurationGenerator.Build(startupConfiguration.Components);
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

            var data = ms.ToArray();
            session.SendMessage(new TransactionMessage(
               message.TransactionId,
               data,
               0,
               data.Length
            ));
         }
      }

      private void HandleGetInitialCommandList(LimitedDSPExSession session, TransactionInitialMessage message) {
         logger.Info("Processing Initial Message");

         throw new NotImplementedException();
         //var commandListConfigurationComponent = startupConfiguration.GetComponentOrNull<CommandListComponent>();
         //var commandList = commandListConfigurationComponent.CommandList;
         //using (var ms = new MemoryStream()) {
         //   using (var writer = new BinaryWriter(ms)) {
         //      writer.Write((uint)commandList.Count);
         //      foreach (var command in commandList) {
         //         writer.WriteLongText(command.Type);
         //         writer.Write(command.Data.Length);
         //         writer.Write(command.Data, 0, command.Data.Length);
         //      }
         //   }
         //
         //   var data = ms.ToArray();
         //   session.SendMessage(new TransactionMessage(message.TransactionId, data, 0, data.Length));
         //   session.DeregisterRITransactionHandler(this);
         //   logger.Info("Sent Initial Command List");
         //}
      }

      private void HandleRemoteLog(LimitedDSPExSession session, TransactionInitialMessage message) {
         using (var ms = streamFactory.CreateMemoryStream(message.DataBuffer, message.DataOffset, message.DataLength))
         using (var reader = ms.Reader) {
            var loggerLevel = reader.ReadUInt32(); // TODO
            var messageLength = reader.ReadUInt32();
            var messageContent = reader.ReadStringOfLength((int)messageLength);
            logger.Debug("REMOTE MESSAGE: L" + loggerLevel + ": " + messageContent.Trim());
         }
      }
   }
}
