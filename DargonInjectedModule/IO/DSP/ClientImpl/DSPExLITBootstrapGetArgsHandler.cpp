#include "stdafx.h"
#include <istream>
#include <sstream>
#include "dargon.hpp"
#include "util.hpp"
#include "binary_reader.hpp"
#include "../DSPEx.hpp"
#include "../DSPExMessage.hpp"
#include "../DSPExInitialMessage.hpp"
#include "../IDSPExSession.hpp"
#include "DSPExLITBootstrapGetArgsHandler.hpp"

using dargon::file_logger;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

DSPExLITBootstrapGetArgsHandler::DSPExLITBootstrapGetArgsHandler(UINT32 transactionId)
   : DSPExLITransactionHandler(transactionId)
{
}

void DSPExLITBootstrapGetArgsHandler::InitializeInteraction(IDSPExSession& session)
{
   file_logger::L(LL_ALWAYS, [](std::ostream& os){ os << "Initialize Bootstrap Get Args Interaction" << std::endl; });
   //std::cout << "Reminder: Currently sending 0 as PID" << std::endl;
   //DWORD pid = 0; 
   DWORD pid = GetProcessId(GetCurrentProcess());
   session.SendMessage(
      DSPExInitialMessage(
         TransactionId,
         DSP_EX_C2S_DIM_BOOTSTRAP_GET_ARGS,
         (PBYTE)&pid,
         4
      )
   );
}

void DSPExLITBootstrapGetArgsHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   file_logger::L(LL_ALWAYS, [](std::ostream& os){ os << "Processing Message of Echo Interaction" << std::endl; });
   
   std::cout << "AE: " << *(UINT32*)message.DataBuffer << " (ptr: " << (void*)message.DataBuffer << ")" << std::endl;
   std::cout << "AF: " << *(UINT32*)(message.DataBuffer + 4) << " (ptr: " << (void*)(message.DataBuffer + 4) << ")" << std::endl;
   std::cout << "AG: " << *(UINT32*)(message.DataBuffer + 8) << " (ptr: " << (void*)(message.DataBuffer + 8) << ")" << std::endl;
   std::cout << "AH: " << *(UINT32*)(message.DataBuffer + 12) << " (ptr: " << (void*)(message.DataBuffer + 12) << ")" << std::endl;

   dargon::binary_reader reader(message.DataBuffer, message.DataLength);

   UINT32 kvpCount = reader.read_uint32();
   file_logger::L(LL_ALWAYS, [=](std::ostream& os){ os << "Got Bootstrap KVP Count " << kvpCount << std::endl; });

   typedef std::pair<std::string, std::string> KeyValuePair;
   std::vector<KeyValuePair> keyValuePairs(kvpCount);
   for (UINT32 i = 0; i < kvpCount; i++)
   {
      UINT32 keyLength = reader.read_uint32();
      char* keyBuffer = new char[keyLength + 1];// + 1 for null terminator
      reader.read_bytes(keyBuffer, keyLength);
      keyBuffer[keyLength] = 0;                 // Set last char to null terminator

      keyValuePairs[i].first = std::string(keyBuffer);

      UINT32 valueLength = reader.read_uint32();
      char* valueBuffer = new char[valueLength + 1];
      reader.read_bytes(valueBuffer, valueLength);
      valueBuffer[valueLength] = 0;

      keyValuePairs[i].second = std::string(valueBuffer);
      
      delete valueBuffer;
      delete keyBuffer;
   }

   UINT32 flagCount = reader.read_uint32();
   file_logger::L(LL_ALWAYS, [=](std::ostream& os){ os << "Got Bootstrap flag Count " << flagCount << std::endl; });
   //std::unique_ptr<std::string[]> flags(new std::string[flagCount]);
   std::vector<std::string> flags(flagCount);
   for (UINT32 i = 0; i < flagCount; i++)
   {
      UINT32 flagLength = reader.read_uint32();
      char* flagBuffer = new char[flagLength + 1];
      reader.read_bytes(flagBuffer, flagLength);
      flagBuffer[flagLength] = 0;

      flags[i] = std::string(flagBuffer);
      delete flagBuffer;
   }
   
   m_properties = std::move(keyValuePairs);
   m_flags = std::move(flags);

   session.DeregisterLITransactionHandler(*this);
   OnCompletion();
}