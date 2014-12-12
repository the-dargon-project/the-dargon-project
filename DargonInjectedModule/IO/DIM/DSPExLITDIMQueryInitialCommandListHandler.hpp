#pragma once

#include "../DSP/DSPExLITransactionHandler.hpp"
#include "DIMCommand.hpp"

namespace dargon { namespace IO { namespace DIM {
   // note: this class never completes on its own! rather, CommandHandler hands it to the command
   // process handler which completes it on its own completion!
   class DSPExLITDIMQueryInitialCommandListHandler : public dargon::IO::DSP::DSPExLITransactionHandler {
      std::vector<dargon::IO::DIM::DIMCommand*> m_commands;

   public:
      DSPExLITDIMQueryInitialCommandListHandler(UINT32 transactionId);
      void InitializeInteraction(dargon::IO::DSP::IDSPExSession& session) override;
      void ProcessMessage(dargon::IO::DSP::IDSPExSession& session, dargon::IO::DSP::DSPExMessage& message) override;

      std::vector<dargon::IO::DIM::DIMCommand*>& ReleaseCommands() { return m_commands;  }
   };
} } }