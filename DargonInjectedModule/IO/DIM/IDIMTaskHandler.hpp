#pragma once

#include <vector>
#include "noncopyable.hpp"

#include "DIMTask.hpp"
#include "DIMTaskTypes.hpp"

namespace dargon { namespace IO { namespace DIM {
   class IDIMTaskHandler : dargon::noncopyable {
   private:

   public:
      virtual ~IDIMTaskHandler() { };
      virtual void Initialize() = 0;
      virtual bool IsTaskTypeSupported(TaskType& type) = 0;
      virtual void ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) = 0;
   };
} } }