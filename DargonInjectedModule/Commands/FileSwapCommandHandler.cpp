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
      command_manager->RegisterTaskHandler(this);
   }
}

bool FileSwapCommandHandler::IsTaskTypeSupported(TaskType& type) { return dargon::iequals(type, TT_FILESWAP);  }

void FileSwapCommandHandler::ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) {
   std::cout << "HANDLING FILE SWAP TASKS" << std::endl;
   for (auto it = begin; it != end; it++) {
      auto task = it->second;
      std::cout << "HANDLE FILE SWAP TASK" << std::endl;

      dargon::binary_reader reader(task->data, task->length);

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