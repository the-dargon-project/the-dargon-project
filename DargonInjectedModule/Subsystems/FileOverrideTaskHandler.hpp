#include "dlc_pch.hpp"
#include <IO/DIM/IDIMTaskHandler.hpp>

namespace Dargon { namespace Subsystems {
   class FileOverrideTaskHandler : public Dargon::IO::DIM::IDIMTaskHandler
   {
   public:
      bool IsTaskTypeSupported(TaskType& type) override;
      void ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) override;
   };
} }