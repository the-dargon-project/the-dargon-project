#include "dlc_pch.hpp"
#include <iostream>
#include <memory>
#include "Dargon.hpp"
#include "Util.hpp"
#include "IO.hpp"
#include "IO/DSP.h"
#include <boost/iostreams/stream.hpp>
#include <boost/iostreams/copy.hpp>
#include <boost/function.hpp>
#include <boost/asio.hpp>
#include "IO/DSP/DSPExFrameTransmitter.hpp"

using namespace dargon;
using namespace dargon::IO;
using namespace dargon::IO::DSP;
using namespace dargon::Util;

void BufferManagerTest();
void HardCodedEchoTest();
void HardCodedEchoTest2();
//void HardCodedEchoTest3();

int main()
{
   try
   {
      Logger::Initialize("C:/DargonLog.log");
      //BufferManagerTest();
      //HardCodedEchoTest();
      //HardCodedEchoTest2();
      //HardCodedEchoTest3();
      //return 0;

      auto ioProxy = std::make_shared<IoProxy>();
      ioProxy->Initialize();

      DSPExNode node(DSPExNodeRole::Client, "DargonTest", ioProxy);
      DSPExNodeSession* session = node.Connect("Dargon");
      std::srand((UINT32)time(NULL));
      BYTE buffer[256];
      memset(buffer, 0x10, sizeof(buffer));
      //while(true)
         Logger::L(LL_ALWAYS, [&](std::ostream& os){ os << "Echo Result: " << session->Echo(buffer, 1) << std::endl; });
      while(true) Sleep(1000);
      return 0;
   }
   catch(std::exception& e)
   {
      std::cout << e.what() << std::endl;
   }
}

bool UIDSetTest()
{
   dargon::Util::UniqueIdentificationSet<UINT32> uidSet(true);
   std::cout << uidSet.TakeUniqueID() << " // " << uidSet << std::endl;
   std::cout << uidSet.TakeUniqueID() << " // " << uidSet << std::endl;
   std::cout << uidSet.TakeUniqueID(10) << " // " << uidSet << std::endl;
   std::cout << uidSet.TakeUniqueID(20) << " // " << uidSet << std::endl;
   std::cout << uidSet.GiveUniqueID(20) << " // " << uidSet << std::endl;
   std::cout << uidSet.GiveUniqueID(0) << " // " << uidSet << std::endl;
   std::cout << uidSet.GiveUniqueID(1) << " // " << uidSet << std::endl;
   std::cout << uidSet.GiveUniqueID(11) << " // " << uidSet << std::endl;
   std::cout << uidSet.GiveUniqueID(10) << " // " << uidSet << std::endl;

   auto uidSet2 = UniqueIdentificationSet<UINT32>(false);
   std::cout << uidSet2.TakeUniqueID() << " // " << uidSet2 << std::endl;
   return true;
}

void HardCodedEchoTest()
{
   std::string pipePath = "\\\\.\\pipe\\dargon";
   auto pipeHandle = CreateFileA(
      pipePath.c_str(),
      GENERIC_READ | GENERIC_WRITE, 
      0,
      NULL, 
      OPEN_EXISTING, 
      FILE_FLAG_WRITE_THROUGH | FILE_FLAG_OVERLAPPED,
      0
   );
   
   if(pipeHandle == INVALID_HANDLE_VALUE)
   {
      auto error = ::GetLastError();
      std::cout << "Error " << error << std::endl;
   }

   if(!SetNamedPipeHandleState(pipeHandle, PIPE_READMODE_BYTE | PIPE_WAIT, NULL, NULL))
   {
      auto error = GetLastError();
      std::cout << "Error " << error << std::endl;
   }

   BYTE opcode = DSP_EX_INIT;
   WriteFile(pipeHandle, &opcode, 1, nullptr, nullptr);
   
   UINT32 length = 9;
   UINT32 tid = 0;
   opcode = DSP_EX_ECHO;
   WriteFile(pipeHandle, &length, 4, nullptr, nullptr);
   WriteFile(pipeHandle, &tid, 4, nullptr, nullptr);
   WriteFile(pipeHandle, &opcode, 1, nullptr, nullptr);

   DWORD bytesRead = 0;
   BYTE buffer[8];
   DWORD bytesToRead = sizeof(buffer);
   ZeroMemory(buffer, bytesToRead);
   while(bytesToRead)
   {
      bytesRead = 0;
      ReadFile(pipeHandle, buffer + (sizeof(buffer) - bytesToRead), bytesToRead, &bytesRead, nullptr);
      bytesToRead -= bytesRead;
   }
   for(int i = 0; i < sizeof(buffer); i++)
      std::cout << i << ": " << (int)buffer[i] << std::endl;
   std::cout << "BytesRead " << bytesRead << std::endl;
}
void HardCodedEchoTest2()
{
   auto ioProxy = std::make_shared<dargon::IO::IoProxy>();
   ioProxy->Initialize();
   IPCObject ipc(ioProxy);
   ipc.Open("dargon", FileAccess::ReadWrite, FileShare::None, false);
   BYTE opcode = DSP_EX_INIT;
   ipc.Write(&opcode, 1);

   UINT32 length = 9;
   UINT32 tid = 0;
   opcode = DSP_EX_ECHO;
   ipc.Write(&length, 4);
   ipc.Write(&tid, 4);
   ipc.Write(&opcode, 1);

   BYTE buffer[8];
   memset(buffer, 127, sizeof(buffer)); //Dummy values
   ipc.ReadBytes(buffer, 8);
   
   for(int i = 0; i < sizeof(buffer); i++)
      std::cout << i << ": " << (int)buffer[i] << std::endl;
}
//void HardCodedEchoTest3()
//{
//   try
//   {
//      DSPExFrameTransmitter* ft = new DSPExFrameTransmitter();
//      ft->Connect("dargon");
//
//      ft->BeginReceivingMessageFrames( 
//         [](BYTE* buffer, int length){
//            std::cout << "length: " << length << std::endl;
//            for(int i = 0; i < length; i++)
//               std::cout << i << ": " << (int)buffer[i] << std::endl;
//         }
//      );
//
//      BYTE opcode = DSP_EX_INIT;
//      ft->SendFrame(&opcode, 0, 1);
//
//      UINT32 length = 9;
//      UINT32 tid = 0;
//      opcode = DSP_EX_ECHO;
//      ft->SendFrame(&length, 0, 4);
//      ft->SendFrame(&tid, 0, 4);
//      ft->SendFrame(&opcode, 0, 1);
//
//      while(true) Sleep(1000);
//   }
//   catch(std::exception& e)
//   {
//      std::cout << e.what() << std::endl;
//   }
//}
void BufferManagerTest()
{
   BufferManager bufferManager(20, DSPConstants::kMaxMessageSize);
   std::vector<dargon::Blob*> buffers;
   for(int i = 0; i < 30; i++)
   {
      auto buffer = bufferManager.TakeBuffer();
      std::cout << "Buffer data pointer " << std::hex << (void*)buffer->data << std::dec << std::endl; 
      buffers.push_back(buffer);
   }
   for(auto buffer : buffers)
   {
      bufferManager.ReturnBuffer(buffer);
   }
}