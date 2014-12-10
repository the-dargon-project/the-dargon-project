#pragma once

#include "stdafx.h"
#include <IO/DIM/IDIMTaskHandler.hpp>
#include "FileSubsystem.hpp"

#define TT_FILESWAP ("FILE_SWAP")

namespace dargon {
   namespace Subsystems {
      class FileSwapTaskHandler : public dargon::IO::DIM::IDIMTaskHandler {
      private:
         FileSubsystem* fileSubsystem;

      public:
         FileSwapTaskHandler(FileSubsystem* fileSubsystem);
         virtual bool IsTaskTypeSupported(TaskType& type) override;
         virtual void ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) override;
      };
   }
}