#pragma once

#include "DIMTask.hpp"
#include "IDIMTaskHandler.hpp"
#include "DIMTaskTypes.hpp"
#include "../DSP/DSPEx.hpp"
#include "../DSP/IDSPExInstructionSet.hpp"
#include "../DSP/DSPExLITransactionHandler.hpp"
#include "../DSP/DSPExRITransactionHandler.hpp"
#include "DSPExRITDIMProcessTaskListHandler.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp";

namespace dargon { namespace IO { namespace DIM {
   class DIMTaskManager;

   class DIMInstructionSet : public dargon::IO::DSP::IDSPExInstructionSet {
   private:
      DIMTaskManager* m_owner;
      DSPExLITDIMQueryInitialTaskListHandler* m_completeOnCompletion;

   public:
      DIMInstructionSet(DIMTaskManager* owner) : m_owner(owner), m_completeOnCompletion(nullptr) { }

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

      void SetCompleteOnCompletion(DSPExLITDIMQueryInitialTaskListHandler* completeOnCompletion)
      {
         m_completeOnCompletion = completeOnCompletion;
      }

   private:

   };
} } }