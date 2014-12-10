#pragma once 

#include "../../dargon.hpp"
#include "../../util.hpp"
#include "../../noncopyable.hpp"
#include "IDSPExSession.hpp"
#include "DSPExMessage.hpp"

namespace dargon { namespace IO { namespace DSP {
   class IDSPExSession;

   class DSPExLITransactionHandler : dargon::noncopyable
   {
   public:
      /// <summary>
      /// The transaction ID associated with this locally initialized transaction handler
      /// </summary>
      const UINT32 TransactionId;

      /// <summary>
      /// When our transaction completes, this countdown event is signalled so that any threads
      /// awaiting this DSPExTransactionHandler's results can continue onwards, with the transaction
      /// results available to them.
      /// </summary>
      dargon::countdown_event& CompletionLatch; //Noncopyable

      /// <summary>
      /// Creates the initial message which begins our interaction.
      /// </summary>
      virtual void InitializeInteraction(IDSPExSession& session) = 0;

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
      /// Initializes a new instance of a DSP Ex Locally Initiated transaction handler, assigning
      /// it the given transaction id.
      /// </summary>
      /// <param name="transactionId">
      /// As we are a locally initialized transaction handler, you will likely have to get the
      /// transaction ID from the DSPManager.
      /// </param>
      DSPExLITransactionHandler(UINT32 transactionId);

      /// <summary>
      /// This method should be invoked when the transaction ends.
      /// </summary>
      void OnCompletion();

   private:
      dargon::countdown_event m_completionLatch;
   };
} } }