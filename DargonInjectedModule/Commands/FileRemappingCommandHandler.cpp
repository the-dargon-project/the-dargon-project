#include "stdafx.h"
#include "util.hpp"
#include "binary_reader.hpp"
#include "FileRemappingCommandHandler.hpp"
#include "../Subsystems/FileSubsystem.hpp"
#include "../Subsystems/RemappedFileOperationProxyFactory.hpp"

using namespace dargon::IO::DIM;
using namespace dargon::Subsystems;

FileRemappingCommandHandler::FileRemappingCommandHandler(
   std::shared_ptr<CommandManager> command_manager,
   std::shared_ptr<FileSubsystem> file_subsystem,
   std::shared_ptr<RemappedFileOperationProxyFactoryFactory> proxy_factory_factory
   ) : command_manager(command_manager), file_subsystem(file_subsystem), proxy_factory_factory(proxy_factory_factory) {}

void FileRemappingCommandHandler::Initialize() {
   if (file_subsystem->IsInitialized()) {
      command_manager->RegisterCommandHandler(this);
   }
}

bool FileRemappingCommandHandler::IsCommandTypeSupported(CommandType& type) { return dargon::iequals(type, CT_FILEREMAP); }

void FileRemappingCommandHandler::ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) {
   std::cout << "HANDLING FILE REMAPPING COMMANDS " << std::endl;
   for (auto it = begin; it != end; it++) {
      auto command = it->second;
//      std::cout << "HANDLE FILE REMAPPING COMMAND " << std::endl;

      dargon::binary_reader reader(command->data, command->length);

      FileIdentifier fileIdentifier;
      reader.read_bytes(&fileIdentifier.targetVolumeSerialNumber, sizeof(fileIdentifier.targetVolumeSerialNumber));
      reader.read_bytes(&fileIdentifier.targetFileIndexHigh, sizeof(fileIdentifier.targetFileIndexHigh));
      reader.read_bytes(&fileIdentifier.targetFileIndexLow, sizeof(fileIdentifier.targetFileIndexLow));

      UINT32 vfmPathLength = reader.read_uint32();
      char* vfmPath = new char[vfmPathLength + 1];
      reader.read_bytes(vfmPath, vfmPathLength);
      vfmPath[vfmPathLength] = 0;

      auto fileProxyFactory = proxy_factory_factory->create(vfmPath);
      file_subsystem->AddFileOverride(fileIdentifier, fileProxyFactory);
   }
}
