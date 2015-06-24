using System;
using System.Collections.Generic;
using Dargon.Transport;
using Dargon.Trinkets.Transport.Helpers;
using ItzWarty;
using ItzWarty.Collections;
using ItzWarty.IO;
using NLog;

namespace Dargon.Trinkets.Transport {
   public interface TrinketDtpServerFactory {

   }

   public class TrinketDtpServerFactoryImpl : TrinketDtpServerFactory {
      private const string DIM_PIPE_NAME_PREFIX = "DargonInjectedModule_";
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private IStreamFactory streamFactory;
      private readonly IDtpNodeFactory transportNodeFactory;
      private BootstrapConfigurationGenerator bootstrapConfigurationGenerator;

      public TrinketDtpServerFactoryImpl(IStreamFactory streamFactory, IDtpNodeFactory transportNodeFactory, BootstrapConfigurationGenerator bootstrapConfigurationGenerator) {
         this.streamFactory = streamFactory;
         this.transportNodeFactory = transportNodeFactory;
         this.bootstrapConfigurationGenerator = bootstrapConfigurationGenerator;
      }

      public TrinketDtpServer Create(TrinketStartupConfiguration startupConfiguration) {
         var pipeName = DIM_PIPE_NAME_PREFIX + startupConfiguration.TargetProcessId;
         logger.Info("Pipe Name: " + pipeName);

         var rithFactoriesByOpcodes = new Dictionary<byte, Func<uint, RemotelyInitializedTransactionHandler>>();
         var instructionSet = new DictionaryInstructionSet(rithFactoriesByOpcodes);
         var transportNode = transportNodeFactory.CreateNode(NodeRole.Server, pipeName, instructionSet.Wrap());
         var wrapper = new TrinketDtpServerImpl(streamFactory, bootstrapConfigurationGenerator, transportNode, startupConfiguration);
         wrapper.Preinitialize(rithFactoriesByOpcodes);
         return wrapper;
      }
   }
}