#include "stdafx.h"
#include "FileSubsystem.hpp"
#include "FileOverrideTaskHandler.hpp"

using namespace dargon::Subsystems;

bool FileOverrideTaskHandler::IsCommandTypeSupported(std::string& type)
{
   return false;
   // return type == CT_FILESWAP;
}

void FileOverrideTaskHandler::ProcessCommands(DIMHandlerToCommandsMap::iterator& begin, DIMHandlerToCommandsMap::iterator& end)
{
   for (auto it = begin; it != end; it++)
   {
      //FileSubsystem::AddFileOverride()
      delete (*it).second;
   }
}