#pragma once

#include <mutex>
#include <thread>
#include <functional>
#include <condition_variable>
#include <boost/signals2.hpp>
#include <boost/thread/barrier.hpp>

#include "../../Dargon.hpp"
#include "../IPCObject.hpp"
#include "../../Util/CountdownEvent.hpp"
#include "DSPExNodeSession.hpp"

namespace Dargon { namespace IO { namespace DSP {
   class DSPExNodeSession;
   class DSPExFrameProcessor;
   typedef std::function<void(DSPExFrameProcessor*)> FrameHandled;
   
   class DSPExFrameProcessor
   {
      typedef std::mutex mutex_type;
      typedef std::unique_lock<mutex_type> lock_type;

      // The DSPEx Client which gives us I/O functions and transaction state.
      DSPExNodeSession& m_client;

      // Pointer to the DSPEx frame (blob) that has been read by the DSPExClient.
      Dargon::Blob* m_pFrame;

      // The thread associated with this frame processor.
      std::thread m_thread;

      // Synchronization Objects
      std::mutex m_mutex;
      std::condition_variable m_condition;

      // Function run when we have finished processing our assigned DSPEx frame
      FrameHandled m_frameHandled;

   public:
      // Initializes a new instance of a DSPEx Frame Processor and associates it with the given client.
      DSPExFrameProcessor(DSPExNodeSession& client, FrameHandled onFrameHandled);

      // Assigns a DSPEx frame buffer to the DSPEx frame processor.
      void AssignFrame(Dargon::Blob* frame);

      // Gets the DSPEx frame buffer assigned to the given frame processor and resets it to nullptr.
      Dargon::Blob* GetAndResetAssignedFrame();

   private:
      static unsigned int WINAPI StaticThreadStart(void* pThis);
      void ThreadStart();
      void RunDSPExIteration();
   };
} } };