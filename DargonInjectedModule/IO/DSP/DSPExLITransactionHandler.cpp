#include "dargon.hpp"
#include "DSPExLITransactionHandler.hpp"
using namespace dargon::IO::DSP;

DSPExLITransactionHandler::DSPExLITransactionHandler(UINT32 transactionId)
   : TransactionId(transactionId), 
     m_completionLatch(1),
     CompletionLatch(m_completionLatch)
{
}

void DSPExLITransactionHandler::OnCompletion()
{
   m_completionLatch.signal();
}