#include "stdafx.h"
#include <unordered_set>
#include <unordered_map>
#include "CommandManager.hpp"
#include "DIMInstructionSet.hpp"
#include "DSPExLITDIMQueryInitialCommandListHandler.hpp"
using namespace dargon;
using namespace dargon::IO::DIM;

CommandManager::CommandManager(
   std::shared_ptr<dargon::IO::DSP::DSPExNodeSession> session, 
   std::shared_ptr<Configuration> configuration
) : session(session), configuration(configuration) {
}

void CommandManager::Initialize() {
   if (configuration->IsFlagSet(Configuration::EnableCommandListFlag)) {
      std::cout << "Registering DIM Command Manager Instruction Set" << std::endl;
      session->AddInstructionSet(new DIMInstructionSet(this));

      std::cout << "Querying Initial DIM Command List" << std::endl;
      auto transactionId = session->TakeLocallyInitializedTransactionId();
      auto handler = std::make_shared<DSPExLITDIMQueryInitialCommandListHandler>(transactionId);
      session->RegisterAndInitializeLITransactionHandler(*handler.get());

      std::cout << "Waiting for initial DIM Command List" << std::endl;
      handler->CompletionLatch.wait();

      std::cout << "Processing Initial DIM Command List... " << std::endl;
      auto commands = handler->ReleaseCommands();
      ProcessCommands(commands);

      std::cout << "Initial DIM Command List processed." << std::endl;
   }
}

void CommandManager::RegisterCommandHandler(IDIMCommandHandler* handler) {
   LockType lock(m_mutex);
   m_handlers.insert(handler);
}

void CommandManager::UnregisterCommandHandler(IDIMCommandHandler* handler) {
   LockType lock(m_mutex);
   m_handlers.erase(handler);
}

void CommandManager::ProcessCommands(std::vector<DIMCommand*>& commands) {
   LockType lock(m_mutex);

   // enumerate all command types
   std::unordered_set<CommandType> CommandTypes;
   for (auto command : commands)
      CommandTypes.insert(command->type);

   // determine what handlers handle the command types
   std::unordered_map<CommandType, IDIMCommandHandler*> typeToHandler;
   for (auto type : CommandTypes) {
      for (auto handler : m_handlers) {
         if (handler->IsCommandTypeSupported(type)) {
            typeToHandler.insert(std::pair<CommandType, IDIMCommandHandler*>(type, handler));
            break;
         }
      }
   }

   // determine what commands are handled by a handler
   DIMHandlerToCommandsMap handlerToCommands;
   std::unordered_set<DIMCommand*> uncategorizedCommands;
   for (auto command : commands) {
      auto handler = typeToHandler.find(command->type);
      if (handler == typeToHandler.end())
         uncategorizedCommands.insert(command);
      else {
         handlerToCommands.insert(std::pair<IDIMCommandHandler*, DIMCommand*>(handler->second, command));
      }
   }

   // pass commands to handler
   for (auto handler : m_handlers)
      handler->ProcessCommands(handlerToCommands.find(handler), handlerToCommands.end());
}