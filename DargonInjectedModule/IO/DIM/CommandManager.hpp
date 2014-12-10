#pragma once

#include <memory>
#include <mutex>
#include <unordered_set>
#include <unordered_map>
#include <vector>

#include "Configuration.hpp"

#include "../DSP/IDSPExInstructionSet.hpp"
#include "../DSP/DSPExRITransactionHandler.hpp"
#include "../DSP/DSPExNodeSession.hpp";

#include "DIMTask.hpp"
#include "IDIMTaskHandler.hpp"
#include "DIMTaskTypes.hpp"
#include "DIMInstructionSet.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp"

namespace dargon { namespace IO { namespace DIM {
   class CommandManager {
      typedef std::mutex MutexType;
      typedef std::unique_lock<MutexType> LockType;

   private:
      std::shared_ptr<dargon::IO::DSP::DSPExNodeSession> session;
      std::shared_ptr<dargon::Configuration> configuration;
      std::unordered_set<IDIMTaskHandler*> m_handlers;
      MutexType m_mutex;
      
   public:
      CommandManager(std::shared_ptr<dargon::IO::DSP::DSPExNodeSession> session, std::shared_ptr<dargon::Configuration>);
      void Initialize();
      void RegisterTaskHandler(IDIMTaskHandler* handler);
      void UnregisterTaskHandler(IDIMTaskHandler* handler);
      void ProcessTasks(std::vector<DIMTask*>& tasks);

   private:
      DSPExLITDIMQueryInitialTaskListHandler* ConstructInitialTaskListQueryHandler(UINT32 transactionId);
   };
} } }