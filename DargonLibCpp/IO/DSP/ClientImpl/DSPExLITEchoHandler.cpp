#include "../../../dlc_pch.hpp"
#include "../../../Dargon.hpp"
#include "../../../Util.hpp"
#include "../DSPEx.hpp"
#include "../DSPExMessage.hpp"
#include "../DSPExInitialMessage.hpp"
#include "../IDSPExSession.hpp"
#include "DSPExLITEchoHandler.hpp"
using Dargon::Util::Logger;
using namespace Dargon::IO::DSP;
using namespace Dargon::IO::DSP::ClientImpl;

DSPExLITEchoHandler::DSPExLITEchoHandler(UINT32 transactionId, BYTE* data, UINT32 dataLength)
   : DSPExLITransactionHandler(transactionId),
     ResponseDataMatched(m_responseDataMatched),
     m_data(data),
     m_dataLength(dataLength)
{
}
void DSPExLITEchoHandler::InitializeInteraction(IDSPExSession& session)
{
   Logger::L(LL_ALWAYS, [](std::ostream& os){ os << "Initialize Echo Interaction" << std::endl; });
   session.SendMessage(
      DSPExInitialMessage(
         TransactionId,
         DSP_EX_ECHO,
         m_data,
         m_dataLength
      )
   );
}
void DSPExLITEchoHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   Logger::L(LL_ALWAYS, [](std::ostream& os){ os << "Processing Message of Echo Interaction" << std::endl; });

   bool match = message.DataLength == m_dataLength;
   if(match) // If length matches, memcmp
      match = memcmp(message.DataBuffer, m_data, m_dataLength) == 0;
   m_responseDataMatched = match;
   session.DeregisterLITransactionHandler(*this);
   OnCompletion();
}
