#pragma once

#include <unordered_set>
#include <unordered_map>
#include <vector>
#include <mutex>
#include "../DSP/IDSPExInstructionSet.hpp"
#include "../DSP/DSPExRITransactionHandler.hpp"

#include "DIMTask.hpp"
#include "IDIMTaskHandler.hpp"
#include "DIMTaskTypes.hpp"
#include "DIMInstructionSet.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp"

namespace dargon { namespace IO { namespace DIM {
   class DIMTaskManager {
      typedef std::mutex MutexType;
      typedef std::unique_lock<MutexType> LockType;

   private:
      std::unordered_set<IDIMTaskHandler*> m_handlers;
      DIMInstructionSet* m_instructionSet;
      MutexType m_mutex;
      
   public:
      DIMTaskManager();
      dargon::IO::DSP::IDSPExInstructionSet* ReleaseInstructionSet();
      void RegisterTaskHandler(IDIMTaskHandler* handler);
      DSPExLITDIMQueryInitialTaskListHandler* ConstructInitialTaskListQueryHandler(UINT32 transactionId);
      void ProcessTasks(std::vector<DIMTask*>& tasks);
   };
} } }