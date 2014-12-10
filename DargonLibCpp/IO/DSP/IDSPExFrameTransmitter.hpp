#pragma once

#include <functional>
#include "../../dargon.hpp"

namespace dargon { namespace IO { namespace DSP {
   // Params: Buffer, Length
   typedef std::function<void(BYTE*, int)> FrameReceivedHandler;
   typedef std::function<void()> FrameSendCompleteHandler;

   class IDSPExFrameTransmitter
   {
   public:
      /// <summary>
      /// Begins receiving message frames.
      /// </summary>
      /// <param name="onFrameReceived">
      /// When a message frame is received, this callback is invoked.
      /// </param>
      virtual void BeginReceivingMessageFrames(FrameReceivedHandler onFrameReceived, int threadCount) = 0;
      
      /// <summary>
      /// Sends a raw buffer to the remote endpoint
      /// </summary>
      /// <param name="buffer">
      /// The buffer which contains the data we're sending
      /// </param>
      /// <param name="offset">
      /// The offset in the buffer where our data starts
      /// </param>
      /// <param name="size">
      /// The length of the data in our buffer
      /// </param>
      virtual void SendFrame(void* buffer, UINT32 offset, UINT32 length) = 0;
   };
} } }