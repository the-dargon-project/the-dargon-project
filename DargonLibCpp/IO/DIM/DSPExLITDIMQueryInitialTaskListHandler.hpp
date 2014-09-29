#pragma once

#include "../DSP/DSPExLITransactionHandler.hpp"

namespace Dargon { namespace IO { namespace DIM {
   // note: this class never completes on its own! rather, TaskManager hands it to the task
   // process handler which completes it on its own completion!
   class DSPExLITDIMQueryInitialTaskListHandler : public Dargon::IO::DSP::DSPExLITransactionHandler {
   public:
      DSPExLITDIMQueryInitialTaskListHandler(UINT32 transactionId);
      void InitializeInteraction(Dargon::IO::DSP::IDSPExSession& session) override;
      void ProcessMessage(Dargon::IO::DSP::IDSPExSession& session, Dargon::IO::DSP::DSPExMessage& message) override { }
   };
} } }