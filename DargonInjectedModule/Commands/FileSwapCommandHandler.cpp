#include "stdafx.h"
#include "util.hpp"
#include "binary_reader.hpp"
#include "FileSwapCommandHandler.hpp"
#include "../Subsystems/FileSubsystem.hpp"

using namespace dargon::IO::DIM;
using namespace dargon::Subsystems;

FileSwapCommandHandler::FileSwapCommandHandler(std::shared_ptr<CommandManager> command_manager, std::shared_ptr<FileSubsystem> file_subsystem)
   : command_manager(command_manager), file_subsystem(file_subsystem) { }

void FileSwapCommandHandler::Initialize() {
   if (file_subsystem->IsInitialized()) {
      command_manager->RegisterCommandHandler(this);
   }
}

bool FileSwapCommandHandler::IsCommandTypeSupported(CommandType& type) { return dargon::iequals(type, CT_FILESWAP);  }

void FileSwapCommandHandler::ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end) {
   std::cout << "HANDLING FILE SWAP COMMANDS " << std::endl;
   for (auto it = begin; it != end; it++) {
      auto command = it->second;
      std::cout << "HANDLE FILE SWAP COMMAND " << std::endl;

      dargon::binary_reader reader(command->data, command->length);

      FileOverrideTargetDescriptor descriptor;
      reader.read_bytes(&descriptor.targetVolumeSerialNumber, sizeof(descriptor.targetVolumeSerialNumber));
      reader.read_bytes(&descriptor.targetFileIndexHigh, sizeof(descriptor.targetFileIndexHigh));
      reader.read_bytes(&descriptor.targetFileIndexLow, sizeof(descriptor.targetFileIndexLow));

      UINT32 replacementPathLength = reader.read_uint32();
      char* replacementPath = new char[replacementPathLength + 1];
      reader.read_bytes(replacementPath, replacementPathLength);
      replacementPath[replacementPathLength] = 0;

      FileOverride fileOverride;
      fileOverride.isFullSwap = true;
      fileOverride.pOverrideTree = nullptr;
      fileOverride.replacementPath = replacementPath;

      file_subsystem->AddFileOverride(descriptor, fileOverride);
   }
}