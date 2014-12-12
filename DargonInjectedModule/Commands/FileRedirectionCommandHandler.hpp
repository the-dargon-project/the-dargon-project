#pragma once

#include "stdafx.h"
#include <IO/DIM/IDIMCommandHandler.hpp>
#include "../IO/DIM/CommandManager.hpp"
#include "../Subsystems/FileSubsystem.hpp"
#include "../Subsystems/RedirectedFileOperationProxyFactoryFactory.hpp"

#define CT_FILESWAP ("FILE_REDIRECTION_COMMAND")

namespace dargon {
   namespace Subsystems {
      class FileRedirectionCommandHandler : public dargon::IO::DIM::IDIMCommandHandler {
      private:
         std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager;
         std::shared_ptr<FileSubsystem> file_subsystem;
         std::shared_ptr<RedirectedFileOperationProxyFactoryFactory> proxy_factory_factory;

      public:
         FileRedirectionCommandHandler(std::shared_ptr<dargon::IO::DIM::CommandManager> command_manager, std::shared_ptr<FileSubsystem> file_subsystem, std::shared_ptr<RedirectedFileOperationProxyFactoryFactory> proxy_factory_factory);
         virtual void Initialize() override;
         virtual bool IsCommandTypeSupported(CommandType& type) override;
         virtual void ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) override;
      };
   }
}