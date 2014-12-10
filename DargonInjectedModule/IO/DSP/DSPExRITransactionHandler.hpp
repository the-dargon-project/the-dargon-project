#pragma once 

#include "dargon.hpp"
#include "util.hpp"
#include "noncopyable.hpp"
#include "IDSPExSession.hpp"
#include "DSPExMessage.hpp"
#include "DSPExInitialMessage.hpp"

namespace dargon { namespace IO { namespace DSP {
   class IDSPExSession;

   class DSPExRITransactionHandler : dargon::noncopyable
   {
   public:
      /// <summary>
      /// The transaction ID associated with this locally initialized transaction handler
      /// </summary>
      const UINT32 TransactionId;
      
      /// <summary>
      /// Handles the initial message (server-sent) which begins our transaction.
      /// </summary>
      virtual void ProcessInitialMessage(IDSPExSession& session, DSPExInitialMessage& message) = 0;

      /// <summary>
      /// Processes a message recieved from the remote endpoint
      /// </summary>
      /// <param name="session">
      /// DSPEx session object, which permits us to send messages.
      /// </param>
      /// <param name="message">
      /// The recieved DSPEx message which we are to process.
      /// </param>
      virtual void ProcessMessage(IDSPExSession& session, DSPExMessage& message) = 0;

   protected:
      /// <summary>
      /// Initializes a new instance of a Remotely Initialized Transaction Handler with the given
      /// transactionId.
      /// </summary>
      /// <param name="transactionId"></param>
      DSPExRITransactionHandler(UINT32 transactionId);
   };
} } }