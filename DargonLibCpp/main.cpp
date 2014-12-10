#include "dlc_pch.hpp"
#include <iostream>
#include <memory>
#include "Dargon.hpp"
#include "Util.hpp"
#include "IO.hpp"
#include "IO/DSP.h"
#include "IO/DSP/DSPExFrameTransmitter.hpp"

using namespace dargon;
using namespace dargon::IO;
using namespace dargon::IO::DSP;
using namespace dargon;

void buffer_managerTest();
void HardCodedEchoTest();
void HardCodedEchoTest2();
//void HardCodedEchoTest3();

int main()
{
   try
   {
      file_logger::Initialize("C:/DargonLog.log");
      //buffer_managerTest();
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
         file_logger::L(LL_ALWAYS, [&](std::ostream& os){ os << "Echo Result: " << session->Echo(buffer, 1) << std::endl; });
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
   dargon::unique_id_set<UINT32> uidSet(true);
   std::cout << uidSet.take() << " // " << uidSet << std::endl;
   std::cout << uidSet.take() << " // " << uidSet << std::endl;
   std::cout << uidSet.take(10) << " // " << uidSet << std::endl;
   std::cout << uidSet.take(20) << " // " << uidSet << std::endl;
   std::cout << uidSet.give(20) << " // " << uidSet << std::endl;
   std::cout << uidSet.give(0) << " // " << uidSet << std::endl;
   std::cout << uidSet.give(1) << " // " << uidSet << std::endl;
   std::cout << uidSet.give(11) << " // " << uidSet << std::endl;
   std::cout << uidSet.give(10) << " // " << uidSet << std::endl;

   auto uidSet2 = unique_id_set<UINT32>(false);
   std::cout << uidSet2.take() << " // " << uidSet2 << std::endl;
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
void buffer_managerTest()
{
   buffer_manager buffer_manager(20, DSPConstants::kMaxMessageSize);
   std::vector<dargon::Blob*> buffers;
   for(int i = 0; i < 30; i++)
   {
      auto buffer = buffer_manager.take();
      std::cout << "Buffer data pointer " << std::hex << (void*)buffer->data << std::dec << std::endl; 
      buffers.push_back(buffer);
   }
   for(auto buffer : buffers)
   {
      buffer_manager.give(buffer);
   }
}