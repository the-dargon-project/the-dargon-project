#pragma once

#include "../DSP/DSPExLITransactionHandler.hpp"
#include "DIMTask.hpp"

namespace dargon { namespace IO { namespace DIM {
   // note: this class never completes on its own! rather, TaskManager hands it to the task
   // process handler which completes it on its own completion!
   class DSPExLITDIMQueryInitialTaskListHandler : public dargon::IO::DSP::DSPExLITransactionHandler {
      std::vector<dargon::IO::DIM::DIMTask*> m_tasks;

   public:
      DSPExLITDIMQueryInitialTaskListHandler(UINT32 transactionId);
      void InitializeInteraction(dargon::IO::DSP::IDSPExSession& session) override;
      void ProcessMessage(dargon::IO::DSP::IDSPExSession& session, dargon::IO::DSP::DSPExMessage& message) override;

      std::vector<dargon::IO::DIM::DIMTask*>& ReleaseTasks() { return m_tasks;  }
   };
} } }