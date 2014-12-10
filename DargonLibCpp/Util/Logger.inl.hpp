#include "Logger.hpp"
void dargon::util::Logger::L(UINT32 loggerLevel, LoggingFunction logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}
void dargon::util::Logger::SL(UINT32 loggerLevel, LoggingFunction logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}
void dargon::util::Logger::SNL(UINT32 loggerLevel, LoggingFunction logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}