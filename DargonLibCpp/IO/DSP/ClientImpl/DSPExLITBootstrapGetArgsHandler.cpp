#include "../../../dlc_pch.hpp"
#include <istream>
#include <sstream>
#include <boost/iostreams/stream.hpp>
#include <boost/iostreams/device/array.hpp>
#include <boost/interprocess/streams/bufferstream.hpp>
#include "../../../Dargon.hpp"
#include "../../../Util.hpp"
#include "../DSPEx.hpp"
#include "../DSPExMessage.hpp"
#include "../DSPExInitialMessage.hpp"
#include "../IDSPExSession.hpp"
#include "DSPExLITBootstrapGetArgsHandler.hpp"
using dargon::util::Logger;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

DSPExLITBootstrapGetArgsHandler::DSPExLITBootstrapGetArgsHandler(UINT32 transactionId)
   : DSPExLITransactionHandler(transactionId)
{
}

void DSPExLITBootstrapGetArgsHandler::InitializeInteraction(IDSPExSession& session)
{
   Logger::L(LL_ALWAYS, [](std::ostream& os){ os << "Initialize Bootstrap Get Args Interaction" << std::endl; });
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
   Logger::L(LL_ALWAYS, [](std::ostream& os){ os << "Processing Message of Echo Interaction" << std::endl; });
   
   std::cout << "AE: " << *(UINT32*)message.DataBuffer << " (ptr: " << (void*)message.DataBuffer << ")" << std::endl;
   std::cout << "AF: " << *(UINT32*)(message.DataBuffer + 4) << " (ptr: " << (void*)(message.DataBuffer + 4) << ")" << std::endl;
   std::cout << "AG: " << *(UINT32*)(message.DataBuffer + 8) << " (ptr: " << (void*)(message.DataBuffer + 8) << ")" << std::endl;
   std::cout << "AH: " << *(UINT32*)(message.DataBuffer + 12) << " (ptr: " << (void*)(message.DataBuffer + 12) << ")" << std::endl;

   boost::interprocess::bufferstream input_stream((char*)message.DataBuffer, message.DataLength);

   UINT32 kvpCount;
   input_stream.read((char*)&kvpCount, 4);
   Logger::L(LL_ALWAYS, [=](std::ostream& os){ os << "Got Bootstrap KVP Count " << kvpCount << std::endl; });

   typedef std::pair<std::string, std::string> KeyValuePair;
   std::vector<KeyValuePair> keyValuePairs(kvpCount);
   for (UINT32 i = 0; i < kvpCount; i++)
   {
      UINT32 keyLength;
      input_stream.read((char*)&keyLength, 4);
      char* keyBuffer = new char[keyLength + 1];// + 1 for null terminator
      input_stream.read(keyBuffer, keyLength);
      keyBuffer[keyLength] = 0;                 // Set last char to null terminator

      keyValuePairs[i].first = std::string(keyBuffer);

      UINT32 valueLength;
      input_stream.read((char*)&valueLength, 4);
      char* valueBuffer = new char[valueLength + 1];
      input_stream.read(valueBuffer, valueLength);
      valueBuffer[valueLength] = 0;

      keyValuePairs[i].second = std::string(valueBuffer);
      
      delete valueBuffer;
      delete keyBuffer;
   }

   UINT32 flagCount;
   input_stream.read((char*)&flagCount, 4);
   Logger::L(LL_ALWAYS, [=](std::ostream& os){ os << "Got Bootstrap flag Count " << flagCount << std::endl; });
   //std::unique_ptr<std::string[]> flags(new std::string[flagCount]);
   std::vector<std::string> flags(flagCount);
   for (UINT32 i = 0; i < flagCount; i++)
   {
      UINT32 flagLength;
      input_stream.read((char*)&flagLength, 4);
      char* flagBuffer = new char[flagLength + 1];
      input_stream.read(flagBuffer, flagLength);
      flagBuffer[flagLength] = 0;

      flags[i] = std::string(flagBuffer);
      delete flagBuffer;
   }
   
   m_properties = std::move(keyValuePairs);
   m_flags = std::move(flags);

   session.DeregisterLITransactionHandler(*this);
   OnCompletion();
}