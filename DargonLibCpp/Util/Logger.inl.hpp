#include "Logger.hpp"
void dargon::Util::Logger::L(UINT32 loggerLevel, LoggingFunction logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}
void dargon::Util::Logger::SL(UINT32 loggerLevel, LoggingFunction logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}
void dargon::Util::Logger::SNL(UINT32 loggerLevel, LoggingFunction logger)
{
   if(s_instance != nullptr)
      s_instance->Log(loggerLevel, logger);
}