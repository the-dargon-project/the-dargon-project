#pragma once

#include "../../../dlc_pch.hpp"
#include "../../../Dargon.hpp"
#include "../../../Util.hpp"
#include "../DSPEx.hpp"
#include "../DSPExMessage.hpp"
#include "../DSPExInitialMessage.hpp"
#include "../IDSPExSession.hpp"
#include "DSPExRITEchoHandler.hpp"
using dargon::Util::Logger;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

DSPExRITEchoHandler::DSPExRITEchoHandler(UINT32 transactionId)
   : DSPExRITransactionHandler(transactionId)
{
}
void DSPExRITEchoHandler::ProcessInitialMessage(IDSPExSession& session, DSPExInitialMessage& message)
{
   auto response = DSPExMessage(TransactionId, message.DataBuffer, message.DataLength);
   Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Created Echo Response of payload pointer " << (void*)message.DataBuffer << " length " << message.DataLength << std::endl; });
   session.SendMessage(response);
   Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Sent Echo Response" << std::endl; });
   session.DeregisterRITransactionHandler(this); // Disposes of this thing as well
   Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Deregistered Echo Response" << std::endl; });
}
void DSPExRITEchoHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   // Just eat the packet (This method call doesn't make sense).
}