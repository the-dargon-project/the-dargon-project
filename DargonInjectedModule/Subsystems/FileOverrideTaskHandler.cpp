#include "stdafx.h"
#include "FileSubsystem.hpp"
#include "FileOverrideTaskHandler.hpp"

using namespace Dargon::Subsystems;

bool FileOverrideTaskHandler::IsTaskTypeSupported(TaskType type)
{
   return type == TT_FILESWAP;
}

void FileOverrideTaskHandler::ProcessTasks(DIMHandlerToTasksMap::iterator begin, DIMHandlerToTasksMap::iterator end)
{
   for (auto it = begin; it != end; it++)
   {
      //FileSubsystem::AddFileOverride()
      delete (*it).second;
   }
}