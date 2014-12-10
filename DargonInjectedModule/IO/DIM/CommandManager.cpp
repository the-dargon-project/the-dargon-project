#include "stdafx.h"
#include <unordered_set>
#include <unordered_map>
#include "CommandManager.hpp"
#include "DIMInstructionSet.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp"
using namespace dargon;
using namespace dargon::IO::DIM;

CommandManager::CommandManager(
   std::shared_ptr<dargon::IO::DSP::DSPExNodeSession> session, 
   std::shared_ptr<Configuration> configuration
) : session(session), configuration(configuration) {
}

void CommandManager::Initialize() {
   if (configuration->IsFlagSet(Configuration::EnableTaskListFlag)) {
      std::cout << "Registering DIM Task Manager Instruction Set" << std::endl;
      session->AddInstructionSet(new DIMInstructionSet(this));

      std::cout << "Querying Initial DIM Task List" << std::endl;
      auto transactionId = session->TakeLocallyInitializedTransactionId();
      auto handler = std::make_shared<DSPExLITDIMQueryInitialTaskListHandler>(transactionId);
      session->RegisterAndInitializeLITransactionHandler(*handler.get());

      std::cout << "Waiting for initial DIM Task List" << std::endl;
      handler->CompletionLatch.wait();

      std::cout << "Processing Initial DIM Task List... " << std::endl;
      auto tasks = handler->ReleaseTasks();
      ProcessTasks(tasks);

      std::cout << "Initial DIM Task List processed." << std::endl;
   }
}

void CommandManager::RegisterTaskHandler(IDIMTaskHandler* handler) {
   LockType lock(m_mutex);
   m_handlers.insert(handler);
}

void CommandManager::UnregisterTaskHandler(IDIMTaskHandler* handler) {
   LockType lock(m_mutex);
   m_handlers.erase(handler);
}

void CommandManager::ProcessTasks(std::vector<DIMTask*>& tasks) {
   LockType lock(m_mutex);

   // enumerate all task types
   std::unordered_set<TaskType> taskTypes;
   for (auto task : tasks)
      taskTypes.insert(task->type);

   // determine what handlers handle the task types
   std::unordered_map<TaskType, IDIMTaskHandler*> typeToHandler;
   for (auto type : taskTypes) {
      for (auto handler : m_handlers) {
         if (handler->IsTaskTypeSupported(type)) {
            typeToHandler.insert(std::pair<TaskType, IDIMTaskHandler*>(type, handler));
            break;
         }
      }
   }

   // determine what tasks are handled by a handler
   DIMHandlerToTasksMap handlerToTasks;
   std::unordered_set<DIMTask*> uncategorizedTasks;
   for (auto task : tasks) {
      auto handler = typeToHandler.find(task->type);
      if (handler == typeToHandler.end())
         uncategorizedTasks.insert(task);
      else {
         handlerToTasks.insert(std::pair<IDIMTaskHandler*, DIMTask*>(handler->second, task));
      }
   }

   // pass tasks to handler
   for (auto handler : m_handlers)
      handler->ProcessTasks(handlerToTasks.find(handler), handlerToTasks.end());
}