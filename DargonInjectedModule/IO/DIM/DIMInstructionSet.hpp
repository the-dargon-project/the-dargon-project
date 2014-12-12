#pragma once

#include "DIMCommand.hpp"
#include "IDIMCommandHandler.hpp"
#include "DIMCommandTypes.hpp"
#include "../DSP/DSPEx.hpp"
#include "../DSP/IDSPExInstructionSet.hpp"
#include "../DSP/DSPExLITransactionHandler.hpp"
#include "../DSP/DSPExRITransactionHandler.hpp"
#include "DSPExRITDIMProcessTaskListHandler.hpp"
#include "DSPExLITDIMQueryInitialCommandListHandler.hpp";

namespace dargon { namespace IO { namespace DIM {
   class CommandManager;

   class DIMInstructionSet : public dargon::IO::DSP::IDSPExInstructionSet {
   private:
      CommandManager* m_owner;
      DSPExLITDIMQueryInitialCommandListHandler* m_completeOnCompletion;

   public:
      DIMInstructionSet(CommandManager* owner) : m_owner(owner), m_completeOnCompletion(nullptr) { }

      virtual bool TryConstructRITHandler(UINT32 transactionId, DSPEx opcode, OUT dargon::IO::DSP::DSPExRITransactionHandler** ppResult) override
      {
         *ppResult = nullptr;
         switch (opcode)
         {
            case DSP_EX_S2C_DIM_RUN_TASKS:
               *ppResult = new dargon::IO::DIM::DSPExRITDIMProcessTaskListHandler(transactionId, m_owner, m_completeOnCompletion);
               m_completeOnCompletion = nullptr;
               break;
         }
         return *ppResult == nullptr;
      }

      void SetCompleteOnCompletion(DSPExLITDIMQueryInitialCommandListHandler* completeOnCompletion)
      {
         m_completeOnCompletion = completeOnCompletion;
      }

   private:

   };
} } }