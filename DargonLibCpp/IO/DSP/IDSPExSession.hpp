#pragma once 

#include "DSPExLITransactionHandler.hpp"
#include "DSPExRITransactionHandler.hpp"
#include "DSPExMessage.hpp"
#include "DSPExInitialMessage.hpp"

namespace Dargon { namespace IO { namespace DSP {
   class DSPExLITransactionHandler;
   class DSPExRITransactionHandler;

   class IDSPExSession
   {
   public:
      /// <summary>
      /// Registers a locally initialized transaction handler so that future messages can be 
      /// routed to it.  This method respects the Transaction Handler's TransactionID property; 
      /// that value is not mutated by this method.
      /// </summary>
      /// <param name="th">
      /// The transaction handler which we are registering.
      /// </param>
      virtual void RegisterAndInitializeLITransactionHandler(DSPExLITransactionHandler& th) = 0;
   
      /// <summary>
      /// Deregisters the given locally initialized transaction handler, freeing its transaction id.
      /// This method is called assuming that the transaction has reached a state where both DSP
      /// endpoints are aware of the transaction ending.  If such is not a case, this method call
      /// will result in a memory leak which will last on the other endpoint until the DSP 
      /// connection is closed.
      /// </summary>
      /// <param name="th"></param>
      virtual void DeregisterLITransactionHandler(DSPExLITransactionHandler& th) = 0;
   
      /// <summary>
      /// Creates a remotely initialized transaction handler for the given opcode
      /// </summary>
      /// <param name="transactionId">
      /// Unique identifier associated with the transaction.
      /// </param>
      /// <param name="opcode">
      /// The opcode associated with the given transaction
      /// </param>
      /// <returns>
      /// The transaction handler, or null if such a transaction handler doesn't exist
      /// </returns>
      virtual DSPExRITransactionHandler* CreateAndRegisterRITransactionHandler(UINT32 transactionId, INT32 opcode) = 0;
   
      /// <summary>
      /// Deregisters the remotely initialized transaction's handler, freeing its transaction id.
      /// This method is called assuming that the transaction has reached a state where both DSP
      /// endpoints are aware of the transaction ending.  If such is not a case, this method call
      /// will result in a memory leak which will last on the other endpoint until the DSP 
      /// connection is closed.
      /// </summary>
      /// <param name="handler"></param>
      virtual void DeregisterRITransactionHandler(DSPExRITransactionHandler* handler) = 0;
   
      /// <summary>
      /// Sends a DSPEX Initial message to the remote endpoint.  Before this method is called, 
      /// DSPEx's RegisterTransaction() method must be called to modify the associated transaction's
      /// transactionId field.
      ///
      /// The method blocks until the data has been written to the underlying stream.  As a result,
      /// you may free any memory associated with the message immediately after the call returns.
      /// </summary>
      /// <param name="message">
      /// The initial message which we are sending.
      /// </param>
      virtual void SendMessage(DSPExInitialMessage& message) = 0;
   
      /// <summary>
      /// Sends a DSPEx message.  This method must be called after DSPExInitialMessage is sent once.
      /// If this is the last DSPEx message to be sent, DeregisterTransaction() must be called by
      /// the associated transaction to free its transactionId.
      ///
      /// The method blocks until the data has been written to the underlying stream.  As a result,
      /// you may free any memory associated with the message immediately after the call returns.
      /// </summary>
      /// <param name="message">
      /// The message which we are sending.
      /// </param>
      virtual void SendMessage(DSPExMessage& message) = 0;
   };
} } } 