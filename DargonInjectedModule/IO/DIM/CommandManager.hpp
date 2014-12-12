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

#include "DIMCommand.hpp"
#include "IDIMCommandHandler.hpp"
#include "DIMCommandTypes.hpp"
#include "DIMInstructionSet.hpp"
#include "DSPExLITDIMQueryInitialCommandListHandler.hpp"

namespace dargon { namespace IO { namespace DIM {
   class CommandManager {
      typedef std::mutex MutexType;
      typedef std::unique_lock<MutexType> LockType;

   private:
      std::shared_ptr<dargon::IO::DSP::DSPExNodeSession> session;
      std::shared_ptr<dargon::Configuration> configuration;
      std::unordered_set<IDIMCommandHandler*> m_handlers;
      MutexType m_mutex;
      
   public:
      CommandManager(std::shared_ptr<dargon::IO::DSP::DSPExNodeSession> session, std::shared_ptr<dargon::Configuration>);
      void Initialize();
      void RegisterCommandHandler(IDIMCommandHandler* handler);
      void UnregisterCommandHandler(IDIMCommandHandler* handler);
      void ProcessCommands(std::vector<DIMCommand*>& 
         s);

   private:
      DSPExLITDIMQueryInitialCommandListHandler* ConstructInitialCommandListQueryHandler(UINT32 transactionId);
   };
} } }