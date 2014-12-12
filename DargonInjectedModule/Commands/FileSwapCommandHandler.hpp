#pragma once

#include "stdafx.h"
#include <IO/DIM/IDIMTaskHandler.hpp>
#include "../IO/DIM/CommandManager.hpp"
#include "../Subsystems/FileSubsystem.hpp"

#define TT_FILESWAP ("FILE_SWAP")

namespace dargon {
   namespace Subsystems {
      class FileSwapCommandHandler : public dargon::IO::DIM::IDIMTaskHandler {
      private:
         std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager;
         std::shared_ptr<FileSubsystem> file_subsystem;

      public:
         FileSwapCommandHandler(std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager, std::shared_ptr<FileSubsystem> file_subsystem);
         virtual void Initialize() override;
         virtual bool IsTaskTypeSupported(TaskType& type) override;
         virtual void ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) override;
      };
   }
}