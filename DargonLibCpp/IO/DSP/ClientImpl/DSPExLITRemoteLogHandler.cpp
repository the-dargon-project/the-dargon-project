#include "dlc_pch.hpp"
#include "DSPExLITRemoteLogHandler.hpp"
#include <string>
#include "../../../Dargon.hpp"
#include "../DSPEx.hpp"
#include "../IDSPExSession.hpp"
#include "../DSPExLITransactionHandler.hpp"

using namespace Dargon::IO;
using namespace Dargon::IO::DSP;
using namespace Dargon::IO::DSP::ClientImpl;

DSPExLITRemoteLogHandler::DSPExLITRemoteLogHandler(UINT32 transactionId, UINT32 loggerLevel, std::string message)
   : DSPExLITransactionHandler(transactionId), m_loggerLevel(loggerLevel), m_message(message)
{
}

void DSPExLITRemoteLogHandler::InitializeInteraction(IDSPExSession& session)
{
   auto messageLength = m_message.length();
   auto bufferSize = 4 + 4 + messageLength + 1;
   BYTE* buffer = new BYTE[bufferSize];
   *(UINT32*)(buffer) = m_loggerLevel;
   *(UINT32*)(buffer + 4) = messageLength;
   memcpy(buffer + 8, m_message.c_str(), messageLength);
   buffer[bufferSize - 1] = 0; //null terminator

   session.SendMessage(
      DSPExInitialMessage(
         TransactionId,
         DSP_EX_C2S_DIM_REMOTE_LOG,
         buffer,
         bufferSize
      )
   );

   delete[] buffer;
   OnCompletion();
   session.DeregisterLITransactionHandler(*this);
}

void DSPExLITRemoteLogHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   // Do nothing, shouldn't happen.
}