#include "../../dlc_pch.hpp"
#include <WinBase.h> // ThreadProc
#include "DSPExFrameProcessor.hpp"
#include "DSPExNodeSession.hpp"
#include "../../Dargon.hpp"
#include "../../Util.hpp"
using namespace Dargon::IO::DSP;
using Dargon::Util::Logger;

DSPExFrameProcessor::DSPExFrameProcessor(DSPExNodeSession& client, FrameHandled onFrameHandled)
   : m_client(client),
     m_frameHandled(onFrameHandled),
     m_pFrame(nullptr)
{
   std::cout << "Constructing Frame Processor" << std::endl;

   // _beginthreadex over std::thread as we permit C-RunTime usage
   _beginthreadex(
      nullptr,				// lpSecurity
      0,					// stack size (default)
      StaticThreadStart,	// thread start	
      this,					// arg
      0,					// init flag
      nullptr				// nullable, receives threadId
   );
   
   std::cout << "Done Constructing Frame Processor" << std::endl;
}

void DSPExFrameProcessor::AssignFrame(Dargon::Blob* frame)
{
   m_pFrame = frame;
   
   std::unique_lock<std::mutex> lock(m_mutex);
   m_condition.notify_one();
}

Dargon::Blob* DSPExFrameProcessor::GetAndResetAssignedFrame()
{
   auto result = m_pFrame;
   m_pFrame = nullptr;
   return result;
}

unsigned int WINAPI DSPExFrameProcessor::StaticThreadStart(void* pThis)
{
   ((DSPExFrameProcessor*)pThis)->ThreadStart();
   return 0;
}

void DSPExFrameProcessor::ThreadStart()
{
   std::cout << "Entered Frame Processor Thread Start!" << std::endl;
   try
   {
      while(true)
      {
         // Block until AssignFrame reaches barrier too
         std::unique_lock<std::mutex> lock(m_mutex);
         m_condition.wait(lock, [this](){ return m_pFrame != nullptr; });
         
         std::cout << "Frame Processor: just assigned new frame!" << std::endl;
         RunDSPExIteration();

         lock.unlock();
         m_frameHandled(this);
      }
   }catch(std::exception& e) { std::cout << e.what() << std::endl; }
}

void DSPExFrameProcessor::RunDSPExIteration()
{
   BYTE* buffer = m_pFrame->data;
   std::cout << "Frame Processor recieved buffer at location " << std::hex << (void*)buffer 
             << std::dec << " size " << m_pFrame->size << std::endl;

   UINT32 frameSize = *(UINT32*)buffer;
   UINT32 transactionId = *(UINT32*)(buffer + sizeof(frameSize));
   int remainingByteCount = frameSize - sizeof(frameSize) - sizeof(transactionId);

   bool isLocallyInitializedTransaction = (transactionId >> 31) != 0x01; // != for server initiated
   Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Got DSPEx Frame of length " << frameSize << " id " << transactionId << " (Is LIT? " << isLocallyInitializedTransaction << ")" << std::endl; });
   if(isLocallyInitializedTransaction)
   {
      auto transaction = m_client.FindLITransactionHandler(transactionId);
      if(transaction)
      {
         DSPExMessage message(transactionId, buffer + 8, remainingByteCount);
         m_client.DumpToConsole(message);
         transaction->ProcessMessage(m_client, message);
      }
      else
      {
         Logger::L(LL_ERROR, [transactionId](std::ostream& os){ 
            os << "Unrecognized transaction " << transactionId
               << "; eating message frame." << std::endl; 
         });
      }
   }
   else
   {
      auto transaction = m_client.FindRITransactionHandler(transactionId);
      if(transaction)
      {
         DSPExMessage message(transactionId, buffer + 8, remainingByteCount);
         m_client.DumpToConsole(message);
         transaction->ProcessMessage(m_client, message);
      }
      else
      {
         //We're starting a new transaction.  Read the opcode and then the data block.
         BYTE opcode = buffer[8]; //9th byte
         DSPExInitialMessage message(transactionId, opcode, buffer + 9, remainingByteCount - 1);
         m_client.DumpToConsole(message);
         
         Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Creating transaction handler for TransactionId " << transactionId << " opcode " << (int)opcode << std::endl; });
         auto transaction = m_client.CreateAndRegisterRITransactionHandler(transactionId, (int)opcode);
         if(transaction) // Null if unsupported
         {
            Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Created transaction handler for TransactionId " << transaction->TransactionId << std::endl; });
            transaction->ProcessInitialMessage(m_client, message);
         }
         else
         {
            Logger::L(LL_ERROR, [opcode](std::ostream& os){
               os << "DSPExClient did not have RITHandler support for opcode " << (int)opcode << std::endl;
            });
         }
      }
   }
}
