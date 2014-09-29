#pragma once 

#include <string>
#include <memory>
#include <mutex>

#include "Dargon.hpp"
#include "Util/CountdownEvent.hpp"
#include "IO/DSP/DSPEx.hpp"
#include "IO/DSP/IDSPExSession.hpp"
#include "IO/DSP/DSPExRITransactionHandler.hpp"
#include "IO/DIM/DIMTask.hpp"

namespace Dargon { namespace IO { namespace DIM {
   class DIMTaskManager;

   class DSPExRITDIMProcessTaskListHandler : public Dargon::IO::DSP::DSPExRITransactionHandler
   {
   private:
      DIMTaskManager* m_owner;

      // When set to true, the first message (header) has been received and m_overrides is sized
      // to fit overrides.
      volatile bool m_headerReceived;
      volatile UINT32 m_taskCount;
      std::vector<Dargon::IO::DIM::DIMTask*> m_tasks;

      // Stops many threads from simultaneously filling the overrides vector
      std::mutex m_fillMutex;
      Dargon::Util::CountdownEvent m_headerReceivedLatch;

      // We can mark another DSPExLITHandler as completed on our own completion.
      // This is used for the LITH which blocks until we've processed a task list.
      Dargon::IO::DSP::DSPExLITransactionHandler* m_completeOnCompletion;

   public:
      DSPExRITDIMProcessTaskListHandler(UINT32 transactionId, DIMTaskManager* owner) : DSPExRITDIMProcessTaskListHandler(transactionId, owner, nullptr) { }
      DSPExRITDIMProcessTaskListHandler(UINT32 transactionId, DIMTaskManager* owner, Dargon::IO::DSP::DSPExLITransactionHandler* completeOnCompletion);

      void ProcessInitialMessage(Dargon::IO::DSP::IDSPExSession& session, Dargon::IO::DSP::DSPExInitialMessage& message) override;
      void ProcessMessage(Dargon::IO::DSP::IDSPExSession& session, Dargon::IO::DSP::DSPExMessage& message) override;
   };
} } } 