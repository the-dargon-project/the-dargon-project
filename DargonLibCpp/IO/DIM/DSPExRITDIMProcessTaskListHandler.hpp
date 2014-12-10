#pragma once 

#include <string>
#include <memory>
#include <mutex>

#include "Dargon.hpp"
#include "countdown_event.hpp"
#include "IO/DSP/DSPEx.hpp"
#include "IO/DSP/IDSPExSession.hpp"
#include "IO/DSP/DSPExRITransactionHandler.hpp"
#include "IO/DIM/DIMTask.hpp"

namespace dargon { namespace IO { namespace DIM {
   class DIMTaskManager;

   class DSPExRITDIMProcessTaskListHandler : public dargon::IO::DSP::DSPExRITransactionHandler
   {
   private:
      DIMTaskManager* m_owner;

      // When set to true, the first message (header) has been received and m_overrides is sized
      // to fit overrides.
      bool m_headerReceived;
      uint32_t m_taskCount;
      std::vector<dargon::IO::DIM::DIMTask*> m_tasks;

      // Stops many threads from simultaneously filling the overrides vector
      std::mutex m_fillMutex;
      dargon::countdown_event m_headerReceivedLatch;

      // We can mark another DSPExLITHandler as completed on our own completion.
      // This is used for the LITH which blocks until we've processed a task list.
      dargon::IO::DSP::DSPExLITransactionHandler* m_completeOnCompletion;

   public:
      DSPExRITDIMProcessTaskListHandler(UINT32 transactionId, DIMTaskManager* owner) : DSPExRITDIMProcessTaskListHandler(transactionId, owner, nullptr) { }
      DSPExRITDIMProcessTaskListHandler(UINT32 transactionId, DIMTaskManager* owner, dargon::IO::DSP::DSPExLITransactionHandler* completeOnCompletion);

      void ProcessInitialMessage(dargon::IO::DSP::IDSPExSession& session, dargon::IO::DSP::DSPExInitialMessage& message) override;
      void ProcessMessage(dargon::IO::DSP::IDSPExSession& session, dargon::IO::DSP::DSPExMessage& message) override;
   };
} } } 