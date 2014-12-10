#include "../../dlc_pch.hpp"
#include <sstream>
#include "../../Init/BootstrapContext.hpp"
#include "../../Util.hpp"
#include "../IPCObject.hpp"
#include "DSP.hpp"
#include "DSPConstants.hpp"
#include "DSPExFrameTransmitter.hpp"
#include "DSPExNode.hpp"
#include "DSPExNodeSession.hpp"
#include "DefaultDSPExInstructionSet.hpp"
#include "ClientImpl/DSPExLITEchoHandler.hpp"

// DSPExHandler Implementations
#include "ClientImpl/DSPExLITEchoHandler.hpp"
#include "ClientImpl/DSPExRITEchoHandler.hpp"
#include "ClientImpl/DSPExLITBootstrapGetArgsHandler.hpp"
#include "ClientImpl/DSPExLITRemoteLogHandler.hpp"

using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;
using namespace dargon::util;

bool DSPExNodeSession::kDebugEnabled = true;
int DSPExNodeSession::kFrameProcessorCount = 2;
int DSPExNodeSession::kFrameProcessorLimit = 16;

DSPExNodeSession::DSPExNodeSession(DSPExNode* pNode, std::shared_ptr<dargon::IO::IoProxy> ioProxy)
   : m_pNode(pNode),
     ioProxy(ioProxy),
     m_ipc(ioProxy),
     m_terminated(false),
     Terminated(m_terminated), 
     m_locallyInitializedUIDSet(0x00000000U, 0x7FFFFFFFU),
     m_remotelyInitializedUIDSet(0x80000000U, 0xFFFFFFFFU),
     m_frameReceivingThreadHandle(0),
     m_frameBufferPool(20, DSPConstants::kMaxMessageSize)
{
   m_terminated = false;
   AddInstructionSet(new DefaultDSPExInstructionSet());
}

bool DSPExNodeSession::Connect(std::string host, UINT32 port)
{
   #pragma message ("DSPExNodeSession.Connect is not implemented!")
   return false;
}

bool DSPExNodeSession::ConnectLocal(const std::string& pipeName)
{
   if(!m_ipc.Open(pipeName, FileAccess::ReadWrite, FileShare::None, false))
      return false;

   // Elevate to DSPEx from the deprecated DSP
   BYTE opcode = DSP_EX_INIT;
   m_ipc.Write(&opcode, 1);
   std::cout << "Sent DSP_EX_INIT opcode" << std::endl;

   // Initialize DSPEx frame processors
   std::cout << "Initializing frame processors" << std::endl;
   for(int i = 0; i < kFrameProcessorCount; i++)
   {
      AddFrameProcessor();
   }
   std::cout << "Done Initializing frame processors!" << std::endl;
   
   // Begin receiving DSPEx frames and assigning them to frame processors
   // m_frameReceivingThread = std::thread(std::bind(&DSPExNodeSession::FrameReceivingThreadStart, this));
   m_frameReceivingThreadHandle = (HANDLE)_beginthreadex(
      nullptr,
      0,
      StaticFrameReceivingThreadStart,
      this,
      0,
      nullptr
   );

   std::cout << "Started Frame Receiving Thread! thread handle " << m_frameReceivingThreadHandle << std::endl;
   return true;
}

void DSPExNodeSession::AddFrameProcessor()
{
   {
      std::unique_lock<std::recursive_mutex> lock(m_processorMutex);
      m_idleFrameProcessors.push_back(
         new DSPExFrameProcessor(
            *this, 
            [this](DSPExFrameProcessor* processor) {
               // Return the processor's frame buffer to the buffer pool
               m_frameBufferPool.ReturnBuffer(processor->GetAndResetAssignedFrame());

               // Move the processor from the Busy collection to the Idle collection
               {
                  std::unique_lock<std::recursive_mutex> lock(m_processorMutex);
                  auto match = std::find(m_busyFrameProcessors.begin(),
                                         m_busyFrameProcessors.end(),
                                         processor);
                  if(match == m_busyFrameProcessors.end())
                     Logger::L(LL_ERROR, [](std::ostream& os){ os << "Didn't find completed processor in busy list!?" << std::endl; });
                  else
                     m_busyFrameProcessors.erase(match);

                  m_idleFrameProcessors.push_back(processor);
               }
            }
         )
      );
      lock.unlock();
   }
}

unsigned int WINAPI DSPExNodeSession::StaticFrameReceivingThreadStart(void* pThis)
{
   //MessageBoxA(NULL, "At static frame receiving Thread Start!", "DIM", MB_OK);
   ((DSPExNodeSession*)pThis)->FrameReceivingThreadStart();
   return 0;
}

void DSPExNodeSession::FrameReceivingThreadStart()
{
   std::cout << "At Frame Receiving Thread Start! pThis: " << (void*)this << std::endl;
   while(!m_terminated)
   {
      std::cout << "At Frame Receiving Thread While Loop! pThis " << (void*)this << std::endl;
      UINT32 length = 0;
      if(!m_ipc.ReadBytes(&length, sizeof(length)))
      {
         Logger::L(LL_ERROR, [=](std::ostream& os){ os << "Read Length error " << m_ipc.GetLastError() << std::endl; });
         return;
      }
      if(length > DSPConstants::kMaxMessageSize)
         Logger::L(LL_WARN, [=](std::ostream& os){ os << "DSPEx Frame Size larger than permitted by specification (length " << length << ")" << std::endl; });
      else
         Logger::L(LL_VERBOSE, [=](std::ostream& os){ os << "Got DSPEx Frame Size " << std::dec << length << "" << std::endl; });

      auto frameBufferBlob = m_frameBufferPool.TakeBuffer(length);
      std::cout << "!! Took Frame Buffer with data location " << std::hex << (void*)frameBufferBlob->data << std::dec << std::endl;

      *(UINT32*)frameBufferBlob->data = length;
      if(!m_ipc.ReadBytes(frameBufferBlob->data + 4, length - 4))
      {
         Logger::L(LL_ERROR, [=](std::ostream& os){ os << "Read Block of length " << length <<  " error " << m_ipc.GetLastError() << std::endl; });
         return;
      }
      
      std::cout << "!! Obtaining Frame Processor for Frame Buffer" << std::endl;
      {
         std::unique_lock<std::recursive_mutex> lock(m_processorMutex);
         if(m_idleFrameProcessors.empty())
            AddFrameProcessor();
         DSPExFrameProcessor* frameProcessor = m_idleFrameProcessors.back();
         m_idleFrameProcessors.pop_back();
         
         m_busyFrameProcessors.push_back(frameProcessor);
         lock.unlock();
         frameProcessor->AssignFrame(frameBufferBlob);
         
         std::cout << "!! Assigned Frame Buffer to Processor" << std::endl;
      }
   }
}
UINT32 DSPExNodeSession::TakeLocallyInitializedTransactionId()
{
   return m_locallyInitializedUIDSet.TakeUniqueID();
}

void DSPExNodeSession::RegisterAndInitializeLITransactionHandler(DSPExLITransactionHandler& th)
{
   LockType lock(m_locallyInitializedTransactionMutex);
   m_locallyInitializedTransactions.insert(LITransactionMap::value_type(th.TransactionId, &th));
   lock.unlock();
   th.InitializeInteraction(*this);
}

void DSPExNodeSession::DeregisterLITransactionHandler(DSPExLITransactionHandler& th)
{
   LockType lock(m_locallyInitializedTransactionMutex);
   m_locallyInitializedTransactions.erase(th.TransactionId);
   lock.unlock();
   m_locallyInitializedUIDSet.GiveUniqueID(th.TransactionId);
}

DSPExLITransactionHandler* DSPExNodeSession::FindLITransactionHandler(UINT32 transactionId)
{
   LockType lock(m_locallyInitializedTransactionMutex);
   auto matches = m_locallyInitializedTransactions.find(transactionId);
   if(matches != m_locallyInitializedTransactions.end())
      return matches->second;
   else
      return nullptr;
}

DSPExRITransactionHandler* DSPExNodeSession::CreateAndRegisterRITransactionHandler(UINT32 transactionId, INT32 opcode)
{
   DSPExRITransactionHandler* pResult = nullptr;
   for (auto instructionSet : m_instructionSets)
   {   
      if(instructionSet->TryConstructRITHandler(transactionId, opcode, &pResult))
         break;
   }
   if (pResult == nullptr)
      m_pNode->TryConstructRITHandler(transactionId, opcode, &pResult);

   if(pResult == nullptr)
      return nullptr;
   else
   {  
      Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Constructed RITHandler for opcode " << (int)opcode << std::endl; });
      LockType lock(m_remotelyInitializedTransactionMutex);
      
      Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Registering RITHandler for opcode " << (int)opcode << std::endl; });
      m_remotelyInitializedTransactions.insert(
         std::make_pair(transactionId, pResult)
      );
      
      Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Freeing Mutex" << std::endl; });
      lock.unlock();
      
      Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Registering transactionId " << transactionId << std::endl; });
      m_remotelyInitializedUIDSet.GiveUniqueID(transactionId);
      return pResult;
   }
}

void DSPExNodeSession::DeregisterRITransactionHandler(DSPExRITransactionHandler* handler)
{
   LockType lock(m_remotelyInitializedTransactionMutex);
   m_remotelyInitializedTransactions.erase(handler->TransactionId);
   lock.unlock();
   m_remotelyInitializedUIDSet.TakeUniqueID(handler->TransactionId);
   delete handler;
}

DSPExRITransactionHandler* DSPExNodeSession::FindRITransactionHandler(UINT32 transactionId)
{
   LockType lock(m_remotelyInitializedTransactionMutex);
   auto matches = m_remotelyInitializedTransactions.find(transactionId);
   if(matches != m_remotelyInitializedTransactions.end())
      return matches->second;
   else
      return nullptr;
}

void DSPExNodeSession::SendMessage(DSPExInitialMessage& message)
{
   //std::cout << "*** ENTER SEND MESSAGE *** " << std::endl;
   #if DEBUG
   if(m_locallyInitializedTransactions.find(message.TransactionId) == 
         m_locallyInitializedTransactions.end())
   {
      throw new std::exception("The locally initialized transaction's id was not registered with the DSPEx Client!");
   }
   #endif
   
   UINT32 messageFrameSize = 4 + 4 + 1 + (UINT32)message.DataLength;
   UINT32 transactionId = message.TransactionId;
   BYTE opcode = message.Opcode;
   std::cout << "!!!" << messageFrameSize << " " << transactionId << std::endl;

   {
      std::cout << "Entering" << std::endl;
      try {
         //std::lock_guard<std::mutex> lock(m_writeMutex);
         m_writeMutex.lock();
         if (true || message.DataLength <= 14) {
            m_ipc.Write(&messageFrameSize, sizeof(messageFrameSize));
            m_ipc.Write(&transactionId, sizeof(transactionId));
            m_ipc.Write(&opcode, sizeof(opcode));
            m_ipc.Write(message.DataBuffer, message.DataLength);
         }
         std::cout << "Exiting" << std::endl;
         m_writeMutex.unlock();
      } catch (const std::exception& e) {
         __debugbreak();
         MessageBoxA(NULL, e.what(), "IPC ERROR!", MB_OK);
      } catch (...) {
         MessageBoxA(NULL, "UNKNOWN ERROR", "IPC ERROR!", MB_OK);
      }
   }
   
   //std::cout << "*** LEAVE SEND MESSAGE *** " << std::endl;
   //DumpToConsole(message);
}

void DSPExNodeSession::SendMessage(DSPExMessage& message)
{
   //std::cout << "*** ENTER SEND MESSAGE *** " << std::endl;

   #if DEBUG
   if((m_locallyInitializedTransactions.find(message.TransactionId) == 
          m_locallyInitializedTransactions.end()) ||
      (m_remotelyInitializedTransactions.find(message.TransactionId) == 
          m_remotelyInitializedTransactions.end()))
   {
      throw new std::exception("The transaction's id was not registered with the DSPEx Client!");
   }
   #endif
   
   UINT32 messageFrameSize = 4 + 4 + (UINT32)message.DataLength;
   UINT32 transactionId = message.TransactionId;
   std::cout << "!!!" << messageFrameSize << " " << transactionId << std::endl;

   {
      std::lock_guard<std::mutex> lock(m_writeMutex);
      m_ipc.Write(&messageFrameSize, sizeof(messageFrameSize));
      m_ipc.Write(&transactionId, sizeof(transactionId));
      m_ipc.Write(message.DataBuffer, message.DataLength);
   }
   
   //std::cout << "*** LEAVE SEND MESSAGE *** " << std::endl;
   //DumpToConsole(message);
}

void DSPExNodeSession::AddInstructionSet(IDSPExInstructionSet* instructionSet)
{
   std::unique_lock<std::mutex>(m_instructionSetMutex);
   m_instructionSets.push_back(instructionSet);
}

bool DSPExNodeSession::Echo(BYTE* buffer, UINT32 length)
{
   UINT32 transactionId = m_locallyInitializedUIDSet.TakeUniqueID();
   DSPExLITEchoHandler handler(transactionId, buffer, length);
   RegisterAndInitializeLITransactionHandler(handler);
   handler.CompletionLatch.Wait();
   return handler.ResponseDataMatched;
}

void DSPExNodeSession::Log(UINT32 loggerLevel, LoggingFunction& logger)
{
   std::stringstream ss;
   logger(ss);

   UINT32 transactionId = m_locallyInitializedUIDSet.TakeUniqueID();
   DSPExLITRemoteLogHandler handler(transactionId, loggerLevel, ss.str());
   RegisterAndInitializeLITransactionHandler(handler);
   handler.CompletionLatch.Wait();
}

void DSPExNodeSession::GetBootstrapArguments(dargon::Init::BootstrapContext* context)
{
   UINT32 transactionId = m_locallyInitializedUIDSet.TakeUniqueID();
   DSPExLITBootstrapGetArgsHandler handler(transactionId);
   RegisterAndInitializeLITransactionHandler(handler);
   std::cout << "Waiting for Bootstrap Arguments handler to complete " << std::endl;
   handler.CompletionLatch.Wait();
   context->ArgumentFlags = std::move(handler.m_flags);
   context->ArgumentProperties = std::move(handler.m_properties);
}

void DSPExNodeSession::DumpToConsole(DSPExMessage& message)
{
   if (!kDebugEnabled) return;
   Logger::SNL(
      LL_VERBOSE,
      [&](std::ostream& os) { 
         os << "Transaction ID: " << message.TransactionId << std::endl; 
         DumpBufferToOutputStream(os, message.DataBuffer, message.DataLength);
      }
   );
}

void DSPExNodeSession::DumpToConsole(DSPExInitialMessage& message)
{
   if (!kDebugEnabled) return;
   Logger::SNL(
      LL_VERBOSE,
      [&](std::ostream& os) { 
         os << "Transaction ID: " << message.TransactionId << " opcode " << message.Opcode << std::endl;
         DumpBufferToOutputStream(os, message.DataBuffer, message.DataLength);
      }
   );
}

void DSPExNodeSession::DumpBufferToOutputStream(std::ostream& os, const BYTE* inputBuffer, UINT32 length)
{
   if (!kDebugEnabled) return;
   for (UINT32 i = 0; i < length; i += 16)
   {
      char buffer[2 * 16 + 7 + 4 + 16 + 1]; //2 char per byte and 7 spaces, 4 spaces of padding, 16 chars and NUL 
      int textOffset = 2 * 16 + 7 + 4;
      // Fill entire buffer with spaces, ending with null terminator
      memset(buffer, ' ', 2 * 16 + 7 + 4);
      memset(buffer + 2 * 16 + 7 + 4, '.', 16); // Might not be filled.
      buffer[sizeof(buffer) - 1] = '\0';

      for (UINT32 offset = 0; offset < 16 && i + offset < length; offset++)
      {
         // Write the byte out in hex display
         int bufferIndex = (offset / 2) * 5 + (offset % 2) * 2;
         BYTE val = inputBuffer[i + offset];
         char lookup[16] = {'0', '1', '2', '3', '4', '5', '6', '7', 
                            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
         char high = lookup[(val >> 4) & 0x0F];
         char low  = lookup[val & 0x0F];
         buffer[bufferIndex] = high;
         buffer[bufferIndex + 1] = low;

         // Write the byte char value out if it's not some ugly symbol
         if(val >= ' ' && val < 127) // 127 = ASCII DEL, then extended ascii follows
            buffer[textOffset + offset] = val;
      }
      os << buffer << std::endl;
   }
}