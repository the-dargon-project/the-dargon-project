#pragma once

#include "stdafx.h"
#include <IO/DIM/IDIMCommandHandler.hpp>
#include "../IO/DIM/CommandManager.hpp"
#include "../Subsystems/FileSubsystem.hpp"

#define CT_FILESWAP ("FILE_SWAP")

namespace dargon {
   namespace Subsystems {
      class FileSwapCommandHandler : public dargon::IO::DIM::IDIMCommandHandler {
      private:
         std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager;
         std::shared_ptr<FileSubsystem> file_subsystem;

      public:
         FileSwapCommandHandler(std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager, std::shared_ptr<FileSubsystem> file_subsystem);
         virtual void Initialize() override;
         virtual bool IsCommandTypeSupported(CommandType& type) override;
         virtual void ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) override;
      };
   }
}