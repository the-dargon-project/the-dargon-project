using System;
using System.Collections.Generic;
using Dargon.Transport;

namespace Dargon.Trinkets.Transport {
   public class DictionaryInstructionSet : IInstructionSet {
      private readonly IReadOnlyDictionary<byte, Func<uint, RemotelyInitializedTransactionHandler>> handlerFactoriesByOpcode;

      public DictionaryInstructionSet(IReadOnlyDictionary<byte, Func<uint, RemotelyInitializedTransactionHandler>> handlerFactoriesByOpcode) {
         this.handlerFactoriesByOpcode = handlerFactoriesByOpcode;
      }

      public bool TryCreateRemotelyInitializedTransactionHandler(byte opcode, uint transactionId, out RemotelyInitializedTransactionHandler handler) {
         Func<uint, RemotelyInitializedTransactionHandler> factory;
         if (!handlerFactoriesByOpcode.TryGetValue(opcode, out factory)) {
            handler = null;
            return false;
         } else {
            handler = factory(transactionId);
            return true;
         }
      }

      // DIM no longer uses these fields. Why do they still exist?
      public bool UseConstructionContext => false;
      public object ConstructionContext => null;
   }
}