#include "stdafx.h"
#include <boost/algorithm/string.hpp>
#include "binary_reader.hpp"
#include "FileSwapTaskHandler.hpp"
#include "FileSubsystem.hpp"

using namespace dargon::Subsystems;

FileSwapTaskHandler::FileSwapTaskHandler(FileSubsystem* fileSubsystem) 
   : fileSubsystem(fileSubsystem)
{
}

bool FileSwapTaskHandler::IsTaskTypeSupported(TaskType& type) { return boost::iequals(type, TT_FILESWAP);  }

void FileSwapTaskHandler::ProcessTasks(DIMHandlerToTasksMap::iterator& begin, DIMHandlerToTasksMap::iterator& end) {
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

      fileSubsystem->AddFileOverride(descriptor, fileOverride);
   }
}