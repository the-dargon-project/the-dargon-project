#include "../../dlc_pch.hpp"
#include <boost/iostreams/stream.hpp>
#include <boost/iostreams/device/array.hpp>
#include <boost/interprocess/streams/bufferstream.hpp>
#include "../DSP/DSPEx.hpp"
#include "../DSP/DSPExInitialMessage.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp"
#include "DIMTask.hpp"

using namespace dargon::IO::DSP;
using namespace dargon::IO::DIM;

DSPExLITDIMQueryInitialTaskListHandler::DSPExLITDIMQueryInitialTaskListHandler(UINT32 transactionId)
   : DSPExLITransactionHandler(transactionId), m_tasks()
{
}

void DSPExLITDIMQueryInitialTaskListHandler::InitializeInteraction(dargon::IO::DSP::IDSPExSession& session)
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

void DSPExLITDIMQueryInitialTaskListHandler::ProcessMessage(dargon::IO::DSP::IDSPExSession& session, dargon::IO::DSP::DSPExMessage& message) 
{ 
   std::cout << "Processing response message of DSPExLITDIMQueryInitialTaskListHandler" << std::endl;

   boost::interprocess::bufferstream input_stream((char*)message.DataBuffer, message.DataLength);

   UINT32 taskCount;
   input_stream.read((char*)&taskCount, sizeof(taskCount));
   std::cout << "Got task count " << taskCount << std::endl;

   for (UINT32 i = 0; i < taskCount; i++) {
      UINT32 typeLength;
      input_stream.read((char*)&typeLength, sizeof(typeLength));

      char* typeBuffer = new char[typeLength + 1]; // +1 for null terminator
      input_stream.read(typeBuffer, typeLength);
      typeBuffer[typeLength] = 0;

      std::string type(typeBuffer);
      //delete typeBuffer;
      std::cout << "Read task type " << type << std::endl;

      UINT32 dataLength;
      input_stream.read((char*)&dataLength, sizeof(dataLength));
      std::cout << "Read task data length " << dataLength << std::endl;
      std::cout << "BYTES ALLOCATED FOR DIMTASK: " << sizeof(DIMTask) + dataLength << std::endl;

      std::cout << "Reading task contents" << std::endl;
      UINT8* taskData = new UINT8[dataLength];
      input_stream.read((char*)taskData, dataLength);
      std::cout << "Read task of type " << type << " and data length " << dataLength << std::endl;

      DIMTask* task = new DIMTask();
      task->type = type;
      task->length = dataLength;
      task->data = taskData;
      m_tasks.push_back(task);
   }

   std::cout << "Read all tasks" << std::endl;
   OnCompletion();
}