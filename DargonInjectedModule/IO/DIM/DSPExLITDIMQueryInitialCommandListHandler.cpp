#include "stdafx.h"
#include "binary_reader.hpp"
#include "../DSP/DSPEx.hpp"
#include "../DSP/DSPExInitialMessage.hpp"
#include "DSPExLITDIMQueryInitialCommandListHandler.hpp"
#include "DIMCommand.hpp"

using namespace dargon::IO::DSP;
using namespace dargon::IO::DIM;

DSPExLITDIMQueryInitialCommandListHandler::DSPExLITDIMQueryInitialCommandListHandler(UINT32 transactionId)
   : DSPExLITransactionHandler(transactionId), m_commands()
{
}

void DSPExLITDIMQueryInitialCommandListHandler::InitializeInteraction(dargon::IO::DSP::IDSPExSession& session)
{
   session.SendMessage(
      DSPExInitialMessage(
         TransactionId,
         DSP_EX_C2S_DIM_READY_FOR_TASKS,
         nullptr,
         0
      )
   );
   std::cout << "Sent DSP_EX_C2S_DIM_READY_FOR_TASKS Message with Transaction Id " << TransactionId << std::endl;
}

void DSPExLITDIMQueryInitialCommandListHandler::ProcessMessage(dargon::IO::DSP::IDSPExSession& session, dargon::IO::DSP::DSPExMessage& message)
{ 
   std::cout << "Processing response message of DSPExLITDIMQueryInitialCommandListHandler" << std::endl;

   dargon::binary_reader reader(message.DataBuffer, message.DataLength);

   UINT32 commandCount = reader.read_uint32();
   std::cout << "Processing " << commandCount << " commands... " << std::endl;

   for (UINT32 i = 0; i < commandCount; i++) {
      std::string type = reader.read_long_text();
//      std::cout << "Read command type " << type << std::endl;

      UINT32 dataLength = reader.read_uint32();
//      std::cout << "Read command data length " << dataLength << std::endl;
//      std::cout << "BYTES ALLOCATED FOR COMMAND: " << sizeof(DIMCommand) + dataLength << std::endl;

//      std::cout << "Reading command contents" << std::endl;
      UINT8* commandData = new UINT8[dataLength];
      reader.read_bytes(commandData, dataLength);
//      std::cout << "Read command of type " << type << " and data length " << dataLength << std::endl;

      DIMCommand* command = new DIMCommand();
      command->type = type;
      command->length = dataLength;
      command->data = commandData;
      m_commands.push_back(command);
   }

   std::cout << "Read all commands" << std::endl;
   OnCompletion();
}