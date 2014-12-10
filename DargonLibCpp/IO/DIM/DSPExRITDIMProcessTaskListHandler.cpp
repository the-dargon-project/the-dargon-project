#include <istream>
#include <sstream>
#include <mutex>
#include "Dargon.hpp"
#include "Util.hpp"
#include "binary_reader.hpp"
#include "IO/DSP/DSPEx.hpp"
#include "IO/DSP/DSPExMessage.hpp"
#include "IO/DSP/DSPExInitialMessage.hpp"
#include "IO/DSP/IDSPExSession.hpp"
#include "DSPExRITDIMProcessTaskListHandler.hpp"

using dargon::file_logger;
using namespace dargon::IO::DIM;
using namespace dargon::IO::DSP;

DSPExRITDIMProcessTaskListHandler::DSPExRITDIMProcessTaskListHandler(
   UINT32 transactionId, DIMTaskManager* owner, 
   dargon::IO::DSP::DSPExLITransactionHandler* completeOnCompletion)
   : DSPExRITransactionHandler(transactionId), 
     m_owner(owner),
     m_completeOnCompletion(completeOnCompletion),
     m_headerReceived(false), 
     m_taskCount(0), 
     m_headerReceivedLatch(1)
{
}

void DSPExRITDIMProcessTaskListHandler::ProcessInitialMessage(IDSPExSession& session, dargon::IO::DSP::DSPExInitialMessage& message) {
   file_logger::L(LL_ALWAYS, [&](std::ostream& os) { os << "Processing Initial Message of DIM.ProcessTaskList"
                                                        << "Buffer: " << std::hex << (void*)message.DataBuffer << " Length: " << std::dec << message.DataLength << std::endl; });

   dargon::binary_reader reader(message.DataBuffer, message.DataLength);
   reader.read_uint32(&m_taskCount);

   m_tasks.resize(m_taskCount);
   m_tasks.clear(); // leaves capacity() the same

   m_headerReceivedLatch.signal();

   if (m_taskCount == 0) {
      if (m_completeOnCompletion)
         m_completeOnCompletion->CompletionLatch.signal();

      session.DeregisterRITransactionHandler(this);
   }
}

void DSPExRITDIMProcessTaskListHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   // Don't process body messages until the header has been recieved
   m_headerReceivedLatch.wait();

   dargon::binary_reader reader((char*)message.DataBuffer, message.DataLength);
   
   while(reader.available() > 0)
   {
      TaskType type = reader.read_long_text();

      UINT32 dataLength = reader.read_uint32();
      UINT8* data = new UINT8[dataLength];
      reader.read_bytes(data, dataLength);

      DIMTask* task = new DIMTask();
      task->type = type;
      task->length = dataLength;
      task->data = data;
      m_tasks.push_back(task);

      std::cout << "Got DIM Task of type " << type << " and length " << dataLength
                  << " Current total count " << m_tasks.size() << "/" << m_taskCount << std::endl;
   }

   if (m_tasks.size() == m_taskCount)
   {
      if (m_completeOnCompletion)
         m_completeOnCompletion->CompletionLatch.signal();
      
      session.DeregisterRITransactionHandler(this);
   }
}