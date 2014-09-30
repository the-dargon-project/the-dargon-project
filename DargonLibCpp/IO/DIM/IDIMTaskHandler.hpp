#pragma once

#include <vector>
#include <boost/noncopyable.hpp>

#include "DIMTask.hpp"
#include "DIMTaskTypes.hpp"

namespace Dargon { namespace IO { namespace DIM {
   class IDIMTaskHandler : boost::noncopyable {
   private:

   public:
      virtual ~IDIMTaskHandler() { };
      virtual bool IsTaskTypeSupported(TaskType& type) = 0;
      virtual void ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) = 0;
   };
} } }