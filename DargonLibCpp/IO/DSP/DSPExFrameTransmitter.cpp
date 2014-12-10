#include "../../dlc_pch.hpp"
#include <string>
#include <thread>
#include <functional>
#include <string>
#include "../../Util.hpp"
#include "DSPExFrameTransmitter.hpp"
#include "DSPConstants.hpp"
using namespace dargon::IO::DSP;
using dargon::util::Logger;

DSPExFrameTransmitter::DSPExFrameTransmitter(std::shared_ptr<dargon::IO::IoProxy> ioProxy)
   : m_ipc(ioProxy)
{
}

bool DSPExFrameTransmitter::Connect(std::string pipeName)
{
   return m_ipc.Open(pipeName, FileAccess::ReadWrite, FileShare::ReadWrite, false);
}

void DSPExFrameTransmitter::BeginReceivingMessageFrames(FrameReceivedHandler onFrameReceived, int threadCount)
{
   for(int i = 0; i < threadCount; i++)
   {
      m_frameTransmitterThreads.push_back(
         std::thread(
            std::bind(&DSPExFrameTransmitter::ReceiveMessageFramesThreadStart, this, onFrameReceived)
         )
      );
   }
}

void DSPExFrameTransmitter::SendFrame(void* buffer, UINT32 offset, UINT32 length)
{
   std::lock_guard<std::mutex> lock(m_writeMutex);
   m_ipc.Write(buffer, offset, length);
}

void DSPExFrameTransmitter::ReceiveMessageFramesThreadStart(FrameReceivedHandler onFrameReceived)
{
   Logger::SNL(LL_INFO, [onFrameReceived](std::ostream& os){ os << "Enter ReceiveMessageFramesThreadStart (onFrameReceived" << "[blah]" << ")" << std::endl; });
   //BYTE buffer[DSPConstants::kMaxMessageSize]; // Place this on the heap and things crash for some reason
   std::vector<BYTE> buffer(DSPConstants::kMaxMessageSize);
   Logger::SNL(LL_INFO, [&](std::ostream& os){ os << "DSPEx Allocated buffer of capacity " << buffer.capacity() << std::endl; });

   try
   {
      while(true) // TODO: Exit reason?
      {
         // Read the length of our DSPEx Message Frame to the first 4 bytes
         //UINT32& length = *(UINT32*)buffer.data();
         if(!m_ipc.ReadBytes(buffer.data(), sizeof(UINT32)))
         {
            auto error = m_ipc.GetLastError();
            
            // Todo: Logger::L Failed to read bytes last error
            Logger::SNL(LL_ERROR, [error](std::ostream& os){ os << "IPC ReadBytes Error Code " << error << std::endl; });
         }

         UINT32 length = *(UINT32*)buffer.data();
         Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Got Frame Length " << length << std::endl; });

         // If the length of the frame is bigger than allowed by the dspex spec, whine.  Then, resize the buffer to accomodate the frame.
         if(length > buffer.capacity())
         {
            Logger::SNL(
               LL_ERROR, 
               [=](std::ostream& os){ 
                  os << "Frame length " << length << " was larger than DSPConstants::kMaxMessageSize of " << DSPConstants::kMaxMessageSize 
                     << "; Resizing Buffer (You're not meeting the DSPEx specification, though)." << std::endl; 
               }
            );

            buffer.reserve(length);
            *(UINT32*)buffer.data() = length; // First four bytes of buffer is length
         }
         // Read the data into our buffer (offset 4 because we've read the frame size earlier; we're basically reading a DSPExMessage/InitialMessage struct)
         m_ipc.ReadBytes((BYTE*)buffer.data() + 4, length - 4); //-4 because we've already read the Length portion of header.
         Logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Got Frame Bytes (Length " << length << ")" << std::endl; });

         // Process the message
         if(onFrameReceived)
            onFrameReceived(buffer.data(), length); 
         else
            Logger::SNL(LL_VERBOSE, [](std::ostream& os){ os << "On Frame Received was Null!" << std::endl; });
         
         //int a;
         //std::cin >> a;
      }
   }
   catch(std::exception& e)
   {
      Logger::L(LL_ERROR, [&e](std::ostream& os){ os << "DSPEx Frame Transmitter thread threw " << e.what() << std::endl; });
   }
}