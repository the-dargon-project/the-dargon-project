#pragma once
#include "file_logger.hpp"
void dargon::file_logger::L(UINT32 file_loggerLevel, LoggingFunction file_logger)
{
   if(s_instance != nullptr)
      s_instance->Log(file_loggerLevel, file_logger);
}
void dargon::file_logger::SL(UINT32 file_loggerLevel, LoggingFunction file_logger)
{
   if(s_instance != nullptr)
      s_instance->Log(file_loggerLevel, file_logger);
}
void dargon::file_logger::SNL(UINT32 file_loggerLevel, LoggingFunction file_logger)
{
   if(s_instance != nullptr)
      s_instance->Log(file_loggerLevel, file_logger);
}