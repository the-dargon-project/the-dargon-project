#include <unordered_set>
#include <unordered_map>
#include "DIMTaskManager.hpp"
#include "DIMInstructionSet.hpp"
#include "DSPExLITDIMQueryInitialTaskListHandler.hpp"
using namespace Dargon::IO::DIM;

DIMTaskManager::DIMTaskManager() 
   : m_instructionSet(new DIMInstructionSet(this))
{
}

Dargon::IO::DSP::IDSPExInstructionSet* DIMTaskManager::ReleaseInstructionSet()
{
   return m_instructionSet;
}

void DIMTaskManager::RegisterTaskHandler(IDIMTaskHandler* handler)
{
   LockType lock(m_mutex);
   m_handlers.insert(handler);
}

DSPExLITDIMQueryInitialTaskListHandler* DIMTaskManager::ConstructInitialTaskListQueryHandler(UINT32 transactionId)
{
   auto handler = new DSPExLITDIMQueryInitialTaskListHandler(transactionId);
   m_instructionSet->SetCompleteOnCompletion(handler);
   return handler;
}

void DIMTaskManager::ProcessTasks(std::vector<DIMTask*>& tasks)
{
   LockType lock(m_mutex);

   // enumerate all task types
   std::unordered_set<TaskType> taskTypes;
   for (auto task : tasks)
      taskTypes.insert(task->type);

   // determine what handlers handle the task types
   std::unordered_map<TaskType, IDIMTaskHandler*> typeToHandler;
   for (auto type : taskTypes)
   {
      for (auto handler : m_handlers)
      {
         if (handler->IsTaskTypeSupported(type))
         {
            typeToHandler.insert(std::pair<TaskType, IDIMTaskHandler*>(type, handler));
            break;
         }
      }
   }

   // determine what tasks are handled by a handler
   DIMHandlerToTasksMap handlerToTasks;
   std::unordered_set<DIMTask*> uncategorizedTasks;
   for (auto task : tasks)
   {
      auto handler = typeToHandler.find(task->type);
      if (handler == typeToHandler.end())
         uncategorizedTasks.insert(task);
      else
      {
         handlerToTasks.insert(std::pair<IDIMTaskHandler*, DIMTask*>(handler->second, task));
      }
   }

   // pass tasks to handler
   for (auto handler : m_handlers)
      handler->ProcessTasks(handlerToTasks.find(handler), handlerToTasks.end());
}