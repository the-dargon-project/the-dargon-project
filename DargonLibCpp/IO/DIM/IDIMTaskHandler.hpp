#pragma once

#include <vector>
#include "../../Util/noncopyable.hpp"

#include "DIMTask.hpp"
#include "DIMTaskTypes.hpp"

namespace dargon { namespace IO { namespace DIM {
   class IDIMTaskHandler : dargon::util::noncopyable {
   private:

   public:
      virtual ~IDIMTaskHandler() { };
      virtual bool IsTaskTypeSupported(TaskType& type) = 0;
      virtual void ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) = 0;
   };
} } }