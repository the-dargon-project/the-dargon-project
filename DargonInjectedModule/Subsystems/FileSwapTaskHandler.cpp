#include "stdafx.h"
#include <boost/algorithm/string.hpp>
#include <boost/iostreams/stream.hpp>
#include <boost/iostreams/device/array.hpp>
#include <boost/interprocess/streams/bufferstream.hpp>
#include "FileSwapTaskHandler.hpp"
#include "FileSubsystem.hpp"

using namespace Dargon::Subsystems;

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

      boost::interprocess::bufferstream input_stream((char*)task->data, task->length);

      FileOverrideTargetDescriptor descriptor;
      input_stream.read((char*)&descriptor.targetVolumeSerialNumber, sizeof(descriptor.targetVolumeSerialNumber));
      input_stream.read((char*)&descriptor.targetFileIndexHigh, sizeof(descriptor.targetFileIndexHigh));
      input_stream.read((char*)&descriptor.targetFileIndexLow, sizeof(descriptor.targetFileIndexLow));

      UINT32 replacementPathLength;
      input_stream.read((char*)&replacementPathLength, sizeof(replacementPathLength));

      char* replacementPath = new char[replacementPathLength + 1];
      input_stream.read(replacementPath, replacementPathLength);
      replacementPath[replacementPathLength] = 0;

      FileOverride fileOverride;
      fileOverride.isFullSwap = true;
      fileOverride.pOverrideTree = nullptr;
      fileOverride.replacementPath = replacementPath;

      fileSubsystem->AddFileOverride(descriptor, fileOverride);
   }
}
