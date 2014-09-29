#include <istream>
#include <sstream>
#include <mutex>
#include <boost/iostreams/stream.hpp>
#include <boost/iostreams/device/array.hpp>
#include <boost/interprocess/streams/bufferstream.hpp>
#include "Dargon.hpp"
#include "Util.hpp"
#include "IO/DSP/DSPEx.hpp"
#include "IO/DSP/DSPExMessage.hpp"
#include "IO/DSP/DSPExInitialMessage.hpp"
#include "IO/DSP/IDSPExSession.hpp"
#include "DSPExRITDIMProcessTaskListHandler.hpp"

using Dargon::Util::Logger;
using namespace Dargon::IO::DIM;
using namespace Dargon::IO::DSP;

DSPExRITDIMProcessTaskListHandler::DSPExRITDIMProcessTaskListHandler(
   UINT32 transactionId, DIMTaskManager* owner, 
   Dargon::IO::DSP::DSPExLITransactionHandler* completeOnCompletion)
   : DSPExRITransactionHandler(transactionId), 
     m_owner(owner),
     m_completeOnCompletion(completeOnCompletion),
     m_headerReceived(false), 
     m_taskCount(0), 
     m_headerReceivedLatch(1)
{
}

void DSPExRITDIMProcessTaskListHandler::ProcessInitialMessage(IDSPExSession& session, Dargon::IO::DSP::DSPExInitialMessage& message)
{
   Logger::L(LL_ALWAYS, [&](std::ostream& os){ os << "Processing Initial Message of DIM.ProcessTaskList"
                                                 << "Buffer: " << std::hex << (void*)message.DataBuffer << " Length: " << std::dec << message.DataLength << std::endl; });
   
   boost::interprocess::bufferstream input_stream((char*)message.DataBuffer, message.DataLength);
   input_stream.read((char*)&m_taskCount, 4);
      
   m_tasks.resize(m_taskCount);
   m_tasks.clear(); // leaves capacity() the same
         
   m_headerReceivedLatch.Signal();

   if (m_taskCount == 0)
   {
      if (m_completeOnCompletion)
         m_completeOnCompletion->CompletionLatch.Signal();
      
      session.DeregisterRITransactionHandler(this);
   }
}

void DSPExRITDIMProcessTaskListHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   // Don't process body messages until the header has been recieved
   m_headerReceivedLatch.Wait();

   boost::interprocess::bufferstream input_stream((char*)message.DataBuffer, message.DataLength);
   
   while(input_stream.tellg() < message.DataLength)
   {
      TaskType type;
      UINT32 length;
      input_stream.read((char*)&type, sizeof(type));
      input_stream.read((char*)&length, sizeof(length));

      DIMTask* pTask = (DIMTask*)new UINT8[sizeof(type) + sizeof(length) + length];
      pTask->type = type;
      pTask->length = length;
      input_stream.read((char*)&pTask->data, length);

      std::cout << "Got DIM Task of type " << type << " and length " << length
                  << " Current total count " << m_tasks.size() << "/" << m_taskCount << std::endl;
   }

   if (m_tasks.size() == m_taskCount)
   {
      if (m_completeOnCompletion)
         m_completeOnCompletion->CompletionLatch.Signal();
      
      session.DeregisterRITransactionHandler(this);
   }
}