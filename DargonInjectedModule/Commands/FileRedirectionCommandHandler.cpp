#include "stdafx.h"
#include "util.hpp"
#include "binary_reader.hpp"
#include "FileRedirectionCommandHandler.hpp"
#include "../Subsystems/FileSubsystem.hpp"
#include "../Subsystems/RedirectedFileOperationProxyFactory.hpp"

using namespace dargon::IO::DIM;
using namespace dargon::Subsystems;

FileRedirectionCommandHandler::FileRedirectionCommandHandler(
   std::shared_ptr<CommandManager> command_manager, 
   std::shared_ptr<FileSubsystem> file_subsystem,
   std::shared_ptr<RedirectedFileOperationProxyFactoryFactory> proxy_factory_factory
) : command_manager(command_manager), file_subsystem(file_subsystem), proxy_factory_factory(proxy_factory_factory) { 
}

void FileRedirectionCommandHandler::Initialize() {
   if (file_subsystem->IsInitialized()) {
      command_manager->RegisterCommandHandler(this);
   }
}

bool FileRedirectionCommandHandler::IsCommandTypeSupported(CommandType& type) { return dargon::iequals(type, CT_FILESWAP);  }

void FileRedirectionCommandHandler::ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) {
   std::cout << "HANDLING FILE REDIRECTION COMMANDS " << std::endl;
   for (auto it = begin; it != end; it++) {
      auto command = it->second;
//      std::cout << "HANDLE FILE REDIRECTION COMMAND " << std::endl;

      dargon::binary_reader reader(command->data, command->length);

      FileIdentifier fileIdentifier;
      reader.read_bytes(&fileIdentifier.targetVolumeSerialNumber, sizeof(fileIdentifier.targetVolumeSerialNumber));
      reader.read_bytes(&fileIdentifier.targetFileIndexHigh, sizeof(fileIdentifier.targetFileIndexHigh));
      reader.read_bytes(&fileIdentifier.targetFileIndexLow, sizeof(fileIdentifier.targetFileIndexLow));

      UINT32 replacementPathLength = reader.read_uint32();
      char* replacementPath = new char[replacementPathLength + 1];
      reader.read_bytes(replacementPath, replacementPathLength);
      replacementPath[replacementPathLength] = 0;

      auto fileProxyFactory = proxy_factory_factory->create(replacementPath);
      file_subsystem->AddFileOverride(fileIdentifier, fileProxyFactory);
   }
}
